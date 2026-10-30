# Terrible Settings Auditor (TSA) for Azure Pipelines

"We screen your app settings before they crash on takeoff."

This extension adds two things to Azure DevOps:

- **A `TerribleSettingsAuditor@1` pipeline task.** It runs your .NET app with `tsa --screen --json`, and the app writes `tsa-report.json`.
- **A TSA tab on the pipeline run.** It sits next to *Summary*, *Tests* and *Code Coverage* and shows the screening report: which luggage (configuration classes) cleared, which items (settings) were flagged, and why.

## Usage

```yaml
- task: TerribleSettingsAuditor@1
  displayName: TSA screening
  inputs:
    path: $(Build.ArtifactStagingDirectory)/app/MyApp.dll
    noJoke: true
```

| Input | Default | Description |
|---|---|---|
| `path` | *(required)* | App `.dll` (run with `dotnet`) or executable. Minimatch patterns are allowed if they match one file. |
| `noJoke` | `false` | Passes `--no-joke`. |
| `quiet` | `false` | Passes `--quiet`. |
| `failOnScreeningFailure` | `true` | When `false`, a failed screening marks the task *succeeded with issues*. |
| `workingDirectory` | app's directory | Where the app runs, so `appsettings.json` is found. |
| `reportPath` | `tsa-report.json` | Report location, relative to the working directory. |
| `arguments` | | Extra arguments appended after `tsa --screen --json`. |
| `timeoutInSeconds` | `300` | Stops the app if it keeps running. `0` disables the timeout. |

**Output variables:** `TsaPassed` (`true`/`false`) and `TsaReportPath`.

Your app has to reference the `TerribleSettingsAuditor` NuGet package and call `await app.UseTerribleSettingsAuditorAsync(args)`.

---

Terrible Settings Auditor is an independent developer tool and is not affiliated with or endorsed by the Transportation Security Administration.
