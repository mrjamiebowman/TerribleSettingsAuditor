"use strict";
var __create = Object.create;
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __getProtoOf = Object.getPrototypeOf;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toESM = (mod, isNodeMode, target) => (target = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(
  // If the importer is in node compatibility mode or this is not an ESM
  // file that has been converted to a CommonJS file using a Babel-
  // compatible transform (i.e. "__esModule" has not been set), then set
  // "default" to the CommonJS "module.exports" for node compatibility.
  isNodeMode || !mod || !mod.__esModule ? __defProp(target, "default", { value: mod, enumerable: true }) : target,
  mod
));

// src/task/index.ts
var fs = __toESM(require("fs"));
var path = __toESM(require("path"));
var import_child_process = require("child_process");
var tl = __toESM(require("azure-pipelines-task-lib/task"));

// src/shared/report.ts
function get(obj, key) {
  if (key in obj) {
    return obj[key];
  }
  const lower = key.toLowerCase();
  const match = Object.keys(obj).find((k) => k.toLowerCase() === lower);
  return match === void 0 ? void 0 : obj[match];
}
function str(obj, key) {
  const v = get(obj, key);
  return v === null || v === void 0 ? void 0 : String(v);
}
function bool(obj, key) {
  const v = get(obj, key);
  return typeof v === "boolean" ? v : void 0;
}
function num(obj, key) {
  const v = get(obj, key);
  return typeof v === "number" ? v : void 0;
}
function arr(obj, key) {
  const v = get(obj, key);
  return Array.isArray(v) ? v.filter((x) => x && typeof x === "object") : [];
}
function normalizeProperty(p) {
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
    reason: str(p, "reason")
  };
}
function normalizeConfiguration(c) {
  const properties = arr(c, "properties").map(normalizeProperty);
  return {
    name: str(c, "name") ?? "(unnamed)",
    namespace: str(c, "namespace"),
    passed: bool(c, "passed") ?? properties.every((p) => p.pass),
    pinned: bool(c, "pinned") ?? false,
    order: num(c, "order"),
    message: str(c, "message"),
    properties
  };
}
function normalizeReport(raw) {
  if (!raw || typeof raw !== "object" || Array.isArray(raw)) {
    throw new Error("tsa-report.json is not a JSON object.");
  }
  const obj = raw;
  const configuration = arr(obj, "configuration").map(normalizeConfiguration);
  return {
    pass: bool(obj, "pass") ?? configuration.every((c) => c.passed),
    configuration
  };
}
function parseReport(text) {
  return normalizeReport(JSON.parse(text.replace(/^﻿/, "")));
}
function getStats(report) {
  const items = report.configuration.flatMap((c) => c.properties);
  return {
    luggage: report.configuration.length,
    luggageFailed: report.configuration.filter((c) => !c.passed).length,
    items: items.length,
    itemsFailed: items.filter((p) => !p.pass).length
  };
}
function sortConfigurations(configs) {
  return [...configs].sort(
    (a, b) => Number(b.pinned) - Number(a.pinned) || (a.order ?? Number.MAX_SAFE_INTEGER) - (b.order ?? Number.MAX_SAFE_INTEGER)
  );
}

// src/task/index.ts
var ATTACHMENT_TYPE = "tsa-report";
var ATTACHMENT_NAME = "tsa-report.json";
function resolveAppPath(input) {
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
    throw new Error(`'${input}' matched ${matches.length} files; narrow the pattern to one:
  ${matches.join("\n  ")}`);
  }
  return matches[0];
}
function splitArgs(line) {
  const args = [];
  const re = /"([^"]*)"|(\S+)/g;
  let m;
  while ((m = re.exec(line)) !== null) {
    args.push(m[1] ?? m[2]);
  }
  return args;
}
function run(tool, args, cwd, timeoutSeconds) {
  return new Promise((resolve2, reject) => {
    console.log(`[command]${tool} ${args.map((a) => /\s/.test(a) ? `"${a}"` : a).join(" ")}`);
    const child = (0, import_child_process.spawn)(tool, args, {
      cwd,
      stdio: "inherit",
      env: { ...process.env, DOTNET_NOLOGO: "1" }
    });
    let timedOut = false;
    const timer = timeoutSeconds > 0 ? setTimeout(() => {
      timedOut = true;
      tl.warning(`TSA did not exit within ${timeoutSeconds}s; stopping the app.`);
      child.kill();
    }, timeoutSeconds * 1e3) : void 0;
    child.on("error", (err) => {
      if (timer) clearTimeout(timer);
      reject(err);
    });
    child.on("close", (exitCode) => {
      if (timer) clearTimeout(timer);
      resolve2({ exitCode, timedOut });
    });
  });
}
function logReport(report) {
  const stats = getStats(report);
  console.log("");
  console.log(
    `TSA screening ${report.pass ? "PASSED" : "FAILED"}: ${stats.luggage - stats.luggageFailed}/${stats.luggage} luggage cleared, ${stats.items - stats.itemsFailed}/${stats.items} items cleared.`
  );
  for (const config of sortConfigurations(report.configuration)) {
    for (const prop of config.properties.filter((p) => !p.pass)) {
      tl.error(`${config.name}.${prop.name}: ${prop.message ?? prop.reason ?? "failed screening"}`);
    }
  }
}
async function main() {
  const appPath = resolveAppPath(tl.getInput("path", true));
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
  if (fs.existsSync(reportFile)) {
    tl.debug(`Removing stale report ${reportFile}`);
    fs.unlinkSync(reportFile);
  }
  const isDll = path.extname(appPath).toLowerCase() === ".dll";
  const tool = isDll ? tl.which("dotnet", true) : appPath;
  const args = [...isDll ? [appPath] : [], "tsa", "--screen", "--json"];
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
  const candidates = [.../* @__PURE__ */ new Set([reportFile, path.resolve(appDir, reportInput)])];
  const found = candidates.find((f) => fs.existsSync(f));
  if (!found) {
    tl.setResult(
      tl.TaskResult.Failed,
      `TSA exited with code ${result.exitCode} but no report was found at ${candidates.join(" or ")}. Make sure the app's TerribleSettingsAuditor package supports '--json'.`
    );
    return;
  }
  let report;
  try {
    report = parseReport(fs.readFileSync(found, "utf8"));
  } catch (err) {
    tl.setResult(tl.TaskResult.Failed, `Could not read ${found}: ${err.message}`);
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
main().catch((err) => tl.setResult(tl.TaskResult.Failed, err.message));
