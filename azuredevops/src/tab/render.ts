import { getStats, sortConfigurations, TsaConfigurationReport, TsaPropertyReport, TsaReport } from "../shared/report";

export interface LoadedReport {
    /** e.g. "Build › TSA: screen MyApp.dll" */
    label: string;
    report: TsaReport;
}

type Child = Node | string | null | undefined | false;

function h<K extends keyof HTMLElementTagNameMap>(
    tag: K,
    attrs: Record<string, string> | null,
    ...children: Child[]
): HTMLElementTagNameMap[K] {
    const el = document.createElement(tag);
    for (const [k, v] of Object.entries(attrs ?? {})) {
        el.setAttribute(k, v);
    }
    el.append(...compact(children));
    return el;
}

function compact(children: Child[]): (Node | string)[] {
    return children.filter((c): c is Node | string => !!c);
}

function badge(pass: boolean, text?: string): HTMLElement {
    return h("span", { class: `badge ${pass ? "pass" : "fail"}` }, text ?? (pass ? "PASS" : "FAIL"));
}

function stat(label: string, value: string, tone?: "pass" | "fail"): HTMLElement {
    return h("div", { class: `stat ${tone ?? ""}` }, h("div", { class: "stat-value" }, value), h("div", { class: "stat-label" }, label));
}

function matches(prop: TsaPropertyReport, config: TsaConfigurationReport, query: string): boolean {
    if (!query) return true;
    return [config.name, config.namespace, prop.name, prop.description, prop.message, prop.reason]
        .some((s) => s?.toLowerCase().includes(query));
}

function propertyRow(prop: TsaPropertyReport): HTMLElement {
    const message = prop.message || prop.reason || "";
    return h(
        "tr",
        { class: prop.pass ? "" : "row-fail" },
        h("td", { class: "col-status" }, badge(prop.pass)),
        h("td", { class: "col-name" }, h("code", null, prop.name)),
        h("td", null, prop.description ?? ""),
        h("td", { class: "col-flag" }, prop.required ? "Yes" : ""),
        h("td", { class: "col-flag" }, prop.secret ? "Yes" : ""),
        h("td", { class: "col-value" }, prop.expose && prop.exposeValue ? h("code", null, prop.exposeValue) : ""),
        h("td", { class: "col-message" }, message)
    );
}

function luggageCard(config: TsaConfigurationReport, props: TsaPropertyReport[], forceOpen: boolean): HTMLElement {
    const cleared = config.properties.filter((p) => p.pass).length;
    const details = h(
        "details",
        { class: `luggage ${config.passed ? "pass" : "fail"}` },
        h(
            "summary",
            null,
            h("span", { class: "chevron", "aria-hidden": "true" }),
            badge(config.passed, config.passed ? "CLEARED" : "FLAGGED"),
            h("span", { class: "luggage-name" }, config.name),
            config.namespace && h("span", { class: "luggage-ns" }, config.namespace),
            config.pinned && h("span", { class: "tag" }, "Pinned"),
            h("span", { class: "luggage-count" }, `${cleared}/${config.properties.length} cleared`)
        ),
        config.message && h("p", { class: "luggage-message" }, config.message),
        props.length === 0
            ? h("p", { class: "muted pad" }, "No luggage items declared.")
            : h(
                  "div",
                  { class: "table-wrap" },
                  h(
                      "table",
                      null,
                      h(
                          "thead",
                          null,
                          h(
                              "tr",
                              null,
                              h("th", { class: "col-status" }, "Status"),
                              h("th", null, "Setting"),
                              h("th", null, "Description"),
                              h("th", null, "Required"),
                              h("th", null, "Secret"),
                              h("th", null, "Value"),
                              h("th", null, "Message")
                          )
                      ),
                      h("tbody", null, ...props.map(propertyRow))
                  )
              )
    );

    details.open = forceOpen || !config.passed;
    return details;
}

interface ViewState {
    selected: number;
    query: string;
    failuresOnly: boolean;
}

/** Renders one or more TSA reports (one per TSA step in the run) into root. */
export function renderReports(root: HTMLElement, reports: LoadedReport[]): void {
    const state: ViewState = {
        selected: Math.max(0, reports.findIndex((r) => !r.report.pass)),
        query: "",
        failuresOnly: false,
    };

    const body = h("div", { class: "report-body" });

    const draw = (): void => {
        const { report } = reports[state.selected];
        const stats = getStats(report);
        const filtering = state.failuresOnly || state.query.length > 0;

        const cards = sortConfigurations(report.configuration)
            .map((config) => {
                const props = [...config.properties]
                    .sort((a, b) => a.name.localeCompare(b.name))
                    .filter((p) => (!state.failuresOnly || !p.pass) && matches(p, config, state.query));
                const keep = !filtering || props.length > 0 || (state.failuresOnly && !config.passed && config.properties.length === 0);
                return keep ? luggageCard(config, props, filtering) : null;
            })
            .filter((c): c is HTMLElement => c !== null);

        body.replaceChildren(
            h(
                "section",
                { class: `banner ${report.pass ? "pass" : "fail"}` },
                h("div", { class: "banner-icon", "aria-hidden": "true" }, report.pass ? "✈️" : "🛑"),
                h(
                    "div",
                    null,
                    h("h1", null, report.pass ? "Cleared for takeoff" : "Additional screening required"),
                    h(
                        "p",
                        null,
                        report.pass
                            ? "Every declared setting passed TSA screening."
                            : `${stats.itemsFailed} setting${stats.itemsFailed === 1 ? "" : "s"} failed screening.`
                    )
                )
            ),
            h(
                "section",
                { class: "stats" },
                stat("Luggage cleared", `${stats.luggage - stats.luggageFailed} / ${stats.luggage}`, stats.luggageFailed ? "fail" : "pass"),
                stat("Items cleared", `${stats.items - stats.itemsFailed} / ${stats.items}`, stats.itemsFailed ? "fail" : "pass"),
                stat("Items flagged", String(stats.itemsFailed), stats.itemsFailed ? "fail" : undefined)
            ),
            cards.length
                ? h("section", { class: "luggage-list" }, ...cards)
                : h("p", { class: "muted pad" }, filtering ? "Nothing matches the current filter." : "This report has no luggage.")
        );
    };

    const search = h("input", { type: "search", placeholder: "Filter settings…", "aria-label": "Filter settings" });
    search.addEventListener("input", () => {
        state.query = search.value.trim().toLowerCase();
        draw();
    });

    const failuresOnly = h("input", { type: "checkbox", id: "failures-only" });
    failuresOnly.addEventListener("change", () => {
        state.failuresOnly = failuresOnly.checked;
        draw();
    });

    const toggleAll = (open: boolean) => () =>
        body.querySelectorAll<HTMLDetailsElement>("details.luggage").forEach((d) => (d.open = open));
    const expand = h("button", { type: "button" }, "Expand all");
    expand.addEventListener("click", toggleAll(true));
    const collapse = h("button", { type: "button" }, "Collapse all");
    collapse.addEventListener("click", toggleAll(false));

    let picker: HTMLElement | null = null;
    if (reports.length > 1) {
        const select = h("select", { "aria-label": "TSA step" });
        reports.forEach((r, i) => {
            const opt = h("option", { value: String(i) }, `${r.report.pass ? "✅" : "❌"} ${r.label}`);
            opt.selected = i === state.selected;
            select.append(opt);
        });
        select.addEventListener("change", () => {
            state.selected = Number(select.value);
            draw();
        });
        picker = h("label", { class: "picker" }, "Step ", select);
    }

    const toolbar = h(
        "div",
        { class: "toolbar" },
        picker,
        search,
        h("label", { class: "check", for: "failures-only" }, failuresOnly, "Failures only"),
        h("span", { class: "spacer" }),
        expand,
        collapse
    );
    const source = reports.length === 1 && h("div", { class: "source muted" }, reports[0].label);

    root.replaceChildren(...compact([toolbar, source, body]));

    draw();
}

export function renderMessage(root: HTMLElement, title: string, detail?: string, tone: "info" | "error" = "info"): void {
    root.replaceChildren(h("div", { class: `empty ${tone}` }, h("h2", null, title), detail && h("p", { class: "muted" }, detail)));
}
