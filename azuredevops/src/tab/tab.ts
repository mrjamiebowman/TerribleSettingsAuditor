import * as SDK from "azure-devops-extension-sdk";
import { getClient } from "azure-devops-extension-api";
import { BuildReference, BuildRestClient, BuildServiceIds, BuildStatus, IBuildPageDataService, TimelineRecord } from "azure-devops-extension-api/Build";
import { parseReport } from "../shared/report";
import { LoadedReport, renderMessage, renderReports } from "./render";

/** Must match ATTACHMENT_TYPE in src/task/index.ts. */
const ATTACHMENT_TYPE = "tsa-report";

const root = document.getElementById("root")!;

/**
 * Attachment links look like
 * .../_apis/build/builds/{buildId}/{timelineId}/{recordId}/attachments/{type}/{name}
 */
function parseAttachmentHref(href: string): { timelineId: string; recordId: string } | null {
    const parts = new URL(href).pathname.split("/");
    const i = parts.lastIndexOf("attachments");
    return i >= 2 ? { timelineId: parts[i - 2], recordId: parts[i - 1] } : null;
}

function labelFor(recordId: string, records: Map<string, TimelineRecord>): string {
    const step = records.get(recordId);
    if (!step) return "TSA report";

    const names = [step.name];
    for (let parent = records.get(step.parentId); parent; parent = records.get(parent.parentId)) {
        // Phase records just repeat the job name
        if (parent.type === "Job" || parent.type === "Stage") names.unshift(parent.name);
    }
    return names.join(" › ");
}

let loadToken = 0;

async function load(build: BuildReference, project: string): Promise<void> {
    const token = ++loadToken;
    const client = getClient(BuildRestClient);

    const attachments = await client.getAttachments(project, build.id, ATTACHMENT_TYPE);

    if (attachments.length === 0) {
        if (token !== loadToken) return;
        const running = build.status !== BuildStatus.Completed;
        renderMessage(
            root,
            running ? "Waiting at the security checkpoint…" : "No TSA report for this run",
            running
                ? "The report will appear here once the TSA step finishes."
                : "The TSA task didn't publish a tsa-report.json. Check the TSA step's log."
        );
        return;
    }

    const timeline = await client.getBuildTimeline(project, build.id).catch(() => undefined);
    const records = new Map((timeline?.records ?? []).map((r) => [r.id, r]));

    const reports: LoadedReport[] = [];
    const failures: string[] = [];

    for (const attachment of attachments) {
        const ids = parseAttachmentHref(attachment._links?.self?.href ?? "");
        if (!ids) {
            failures.push(`${attachment.name}: unrecognised attachment link`);
            continue;
        }

        try {
            const buffer = await client.getAttachment(project, build.id, ids.timelineId, ids.recordId, ATTACHMENT_TYPE, attachment.name);
            reports.push({
                label: labelFor(ids.recordId, records),
                report: parseReport(new TextDecoder("utf-8").decode(buffer)),
            });
        } catch (err) {
            failures.push(`${labelFor(ids.recordId, records)}: ${(err as Error).message}`);
        }
    }

    if (token !== loadToken) return;

    if (reports.length === 0) {
        renderMessage(root, "Couldn't read the TSA report", failures.join("\n"), "error");
        return;
    }

    renderReports(root, reports);
}

function show(build: BuildReference | undefined): void {
    const project = SDK.getWebContext().project?.id;
    if (!build || !project) {
        renderMessage(root, "No pipeline run found", undefined, "error");
        return;
    }
    load(build, project).catch((err: Error) => renderMessage(root, "Couldn't load the TSA report", err.message, "error"));
}

async function initInAzureDevOps(): Promise<void> {
    SDK.init({ applyTheme: true, loaded: false });
    await SDK.ready();

    const config = SDK.getConfiguration();
    if (typeof config.onBuildChanged === "function") {
        // fires on load and whenever the run updates (e.g. the TSA step finishes)
        config.onBuildChanged((build: BuildReference) => show(build));
    } else {
        const pageData = await SDK.getService<IBuildPageDataService>(BuildServiceIds.BuildPageDataService);
        show(pageData.getBuildPageData()?.build);
    }

    await SDK.notifyLoadSucceeded();
}

/** Opened directly (not inside Azure DevOps): act as a local viewer for a tsa-report.json. */
function initStandalone(): void {
    document.documentElement.classList.add("standalone");

    const showText = (text: string, label: string): void => {
        try {
            renderReports(root, [{ label, report: parseReport(text) }]);
        } catch (err) {
            renderMessage(root, "Couldn't read the TSA report", (err as Error).message, "error");
        }
    };

    const reportUrl = new URLSearchParams(location.search).get("report");
    if (reportUrl) {
        fetch(reportUrl)
            .then((r) => (r.ok ? r.text() : Promise.reject(new Error(`${r.status} ${r.statusText}`))))
            .then((text) => showText(text, reportUrl))
            .catch((err: Error) => renderMessage(root, "Couldn't load the TSA report", err.message, "error"));
        return;
    }

    const input = document.createElement("input");
    input.type = "file";
    input.accept = ".json,application/json";
    input.addEventListener("change", async () => {
        const file = input.files?.[0];
        if (file) showText(await file.text(), file.name);
    });

    renderMessage(root, "TSA report viewer", "Open a tsa-report.json to preview it, or pass ?report=<url>.");
    root.querySelector(".empty")?.append(input);
}

if (window.self === window.top) {
    initStandalone();
} else {
    initInAzureDevOps().catch((err: Error) => {
        renderMessage(root, "Couldn't start the TSA tab", err.message, "error");
        SDK.notifyLoadFailed(err);
    });
}
