/**
 * Shape of tsa-report.json, mirroring TerribleSettingsAuditor.Models.ScreeningReport.
 *
 * The .NET side may serialize with PascalCase (default) or camelCase (web defaults),
 * so normalizeReport() reads keys case-insensitively and fills in safe defaults.
 */

export interface TsaPropertyReport {
    name: string;
    description?: string;
    pass: boolean;
    message?: string;
    required?: boolean;
    order?: number;
    secret?: boolean;
    expose?: boolean;
    exposeValue?: string;
    reason?: string;
}

export interface TsaConfigurationReport {
    name: string;
    namespace?: string;
    passed: boolean;
    pinned: boolean;
    order?: number;
    message?: string;
    properties: TsaPropertyReport[];
}

export interface TsaReport {
    pass: boolean;
    configuration: TsaConfigurationReport[];
}

export interface TsaReportStats {
    luggage: number;
    luggageFailed: number;
    items: number;
    itemsFailed: number;
}

type Json = Record<string, unknown>;

function get(obj: Json, key: string): unknown {
    if (key in obj) {
        return obj[key];
    }

    const lower = key.toLowerCase();
    const match = Object.keys(obj).find((k) => k.toLowerCase() === lower);
    return match === undefined ? undefined : obj[match];
}

function str(obj: Json, key: string): string | undefined {
    const v = get(obj, key);
    return v === null || v === undefined ? undefined : String(v);
}

function bool(obj: Json, key: string): boolean | undefined {
    const v = get(obj, key);
    return typeof v === "boolean" ? v : undefined;
}

function num(obj: Json, key: string): number | undefined {
    const v = get(obj, key);
    return typeof v === "number" ? v : undefined;
}

function arr(obj: Json, key: string): Json[] {
    const v = get(obj, key);
    return Array.isArray(v) ? (v.filter((x) => x && typeof x === "object") as Json[]) : [];
}

function normalizeProperty(p: Json): TsaPropertyReport {
    return {
        name: str(p, "name") ?? "(unnamed)",
        description: str(p, "description"),
        pass: bool(p, "pass") ?? false,
        message: str(p, "message"),
        required: bool(p, "required"),
        order: num(p, "order"),
        secret: bool(p, "secret"),
        expose: bool(p, "expose"),
        exposeValue: str(p, "exposeValue"),
        reason: str(p, "reason"),
    };
}

function normalizeConfiguration(c: Json): TsaConfigurationReport {
    const properties = arr(c, "properties").map(normalizeProperty);
    return {
        name: str(c, "name") ?? "(unnamed)",
        namespace: str(c, "namespace"),
        passed: bool(c, "passed") ?? properties.every((p) => p.pass),
        pinned: bool(c, "pinned") ?? false,
        order: num(c, "order"),
        message: str(c, "message"),
        properties,
    };
}

export function normalizeReport(raw: unknown): TsaReport {
    if (!raw || typeof raw !== "object" || Array.isArray(raw)) {
        throw new Error("tsa-report.json is not a JSON object.");
    }

    const obj = raw as Json;
    const configuration = arr(obj, "configuration").map(normalizeConfiguration);

    return {
        pass: bool(obj, "pass") ?? configuration.every((c) => c.passed),
        configuration,
    };
}

export function parseReport(text: string): TsaReport {
    // strip a UTF-8 BOM, which .NET writers commonly emit
    return normalizeReport(JSON.parse(text.replace(/^﻿/, "")));
}

export function getStats(report: TsaReport): TsaReportStats {
    const items = report.configuration.flatMap((c) => c.properties);
    return {
        luggage: report.configuration.length,
        luggageFailed: report.configuration.filter((c) => !c.passed).length,
        items: items.length,
        itemsFailed: items.filter((p) => !p.pass).length,
    };
}

/** Same ordering as the TSA console report: unpinned first, then by order. */
export function sortConfigurations(configs: TsaConfigurationReport[]): TsaConfigurationReport[] {
    return [...configs].sort(
        (a, b) =>
            Number(a.pinned) - Number(b.pinned) ||
            (a.order ?? Number.MAX_SAFE_INTEGER) - (b.order ?? Number.MAX_SAFE_INTEGER)
    );
}
