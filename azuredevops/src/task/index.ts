import * as fs from "fs";
import * as path from "path";
import { spawn } from "child_process";
import * as tl from "azure-pipelines-task-lib/task";
import { getStats, parseReport, sortConfigurations, TsaReport } from "../shared/report";

/** Must match the type the TSA tab requests in src/tab/tab.ts. */
const ATTACHMENT_TYPE = "tsa-report";
const ATTACHMENT_NAME = "tsa-report.json";

interface RunResult {
    exitCode: number | null;
    timedOut: boolean;
}

function resolveAppPath(input: string): string {
    if (!/[*?[\]{}]/.test(input)) {
        const resolved = path.resolve(input);
        tl.checkPath(resolved, "path");
        return resolved;
    }

    const root = tl.getVariable("System.DefaultWorkingDirectory") ?? process.cwd();
    const matches = tl.findMatch(root, input).filter((m) => fs.statSync(m).isFile());

    if (matches.length === 0) {
        throw new Error(`No file matched '${input}' under ${root}.`);
    }
    if (matches.length > 1) {
        throw new Error(`'${input}' matched ${matches.length} files; narrow the pattern to one:\n  ${matches.join("\n  ")}`);
    }

    return matches[0];
}

/** Splits an argument string, honouring double quotes. */
function splitArgs(line: string): string[] {
    const args: string[] = [];
    const re = /"([^"]*)"|(\S+)/g;
    let m: RegExpExecArray | null;
    while ((m = re.exec(line)) !== null) {
        args.push(m[1] ?? m[2]);
    }
    return args;
}

function run(tool: string, args: string[], cwd: string, timeoutSeconds: number): Promise<RunResult> {
    return new Promise((resolve, reject) => {
        console.log(`[command]${tool} ${args.map((a) => (/\s/.test(a) ? `"${a}"` : a)).join(" ")}`);

        const child = spawn(tool, args, {
            cwd,
            stdio: "inherit",
            env: { ...process.env, DOTNET_NOLOGO: "1" },
        });

        let timedOut = false;
        const timer =
            timeoutSeconds > 0
                ? setTimeout(() => {
                      timedOut = true;
                      tl.warning(`TSA did not exit within ${timeoutSeconds}s; stopping the app.`);
                      child.kill();
                  }, timeoutSeconds * 1000)
                : undefined;

        child.on("error", (err) => {
            if (timer) clearTimeout(timer);
            reject(err);
        });

        child.on("close", (exitCode) => {
            if (timer) clearTimeout(timer);
            resolve({ exitCode, timedOut });
        });
    });
}

function logReport(report: TsaReport): void {
    const stats = getStats(report);
    console.log("");
    console.log(
        `TSA screening ${report.pass ? "PASSED" : "FAILED"}: ` +
            `${stats.luggage - stats.luggageFailed}/${stats.luggage} luggage cleared, ` +
            `${stats.items - stats.itemsFailed}/${stats.items} items cleared.`
    );

    for (const config of sortConfigurations(report.configuration)) {
        for (const prop of config.properties.filter((p) => !p.pass)) {
            tl.error(`${config.name}.${prop.name}: ${prop.message ?? prop.reason ?? "failed screening"}`);
        }
    }
}

async function main(): Promise<void> {
    const appPath = resolveAppPath(tl.getInput("path", true)!);
    const noJoke = tl.getBoolInput("noJoke", false);
    const quiet = tl.getBoolInput("quiet", false);
    const failOnScreeningFailure = tl.getBoolInput("failOnScreeningFailure", false);
    const extraArgs = splitArgs(tl.getInput("arguments", false) ?? "");
    const timeoutSeconds = Number(tl.getInput("timeoutInSeconds", false) || "300");

    const appDir = path.dirname(appPath);
    const cwd = path.resolve(tl.getInput("workingDirectory", false) || appDir);
    tl.checkPath(cwd, "workingDirectory");

    const reportInput = tl.getInput("reportPath", false) || "tsa-report.json";
    const reportFile = path.resolve(cwd, reportInput);

    // never publish a report left over from an earlier run
    if (fs.existsSync(reportFile)) {
        tl.debug(`Removing stale report ${reportFile}`);
        fs.unlinkSync(reportFile);
    }

    const isDll = path.extname(appPath).toLowerCase() === ".dll";
    const tool = isDll ? tl.which("dotnet", true) : appPath;
    const args = [...(isDll ? [appPath] : []), "tsa", "--screen", "--json"];
    if (noJoke) args.push("--no-joke");
    if (quiet) args.push("--quiet");
    args.push(...extraArgs);

    const result = await run(tool, args, cwd, Number.isFinite(timeoutSeconds) ? timeoutSeconds : 300);

    if (result.timedOut) {
        tl.setResult(
            tl.TaskResult.Failed,
            `TSA timed out after ${timeoutSeconds}s. Make sure the app calls UseTerribleSettingsAuditorAsync(args).`
        );
        return;
    }

    const candidates = [...new Set([reportFile, path.resolve(appDir, reportInput)])];
    const found = candidates.find((f) => fs.existsSync(f));

    if (!found) {
        tl.setResult(
            tl.TaskResult.Failed,
            `TSA exited with code ${result.exitCode} but no report was found at ${candidates.join(" or ")}. ` +
                "Make sure the app's TerribleSettingsAuditor package supports '--json'."
        );
        return;
    }

    let report: TsaReport;
    try {
        report = parseReport(fs.readFileSync(found, "utf8"));
    } catch (err) {
        tl.setResult(tl.TaskResult.Failed, `Could not read ${found}: ${(err as Error).message}`);
        return;
    }

    tl.addAttachment(ATTACHMENT_TYPE, ATTACHMENT_NAME, found);
    tl.setVariable("TsaPassed", String(report.pass), false, true);
    tl.setVariable("TsaReportPath", found, false, true);

    logReport(report);

    if (!report.pass) {
        const message = "TSA screening failed. See the TSA tab for the full report.";
        tl.setResult(failOnScreeningFailure ? tl.TaskResult.Failed : tl.TaskResult.SucceededWithIssues, message);
    } else if (result.exitCode !== 0) {
        tl.setResult(tl.TaskResult.Failed, `TSA screening passed but the app exited with code ${result.exitCode}.`);
    } else {
        tl.setResult(tl.TaskResult.Succeeded, "TSA screening passed. Cleared for takeoff.");
    }
}

main().catch((err: Error) => tl.setResult(tl.TaskResult.Failed, err.message));
