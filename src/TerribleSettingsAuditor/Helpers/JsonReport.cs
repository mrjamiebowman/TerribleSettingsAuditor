using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using TerribleSettingsAuditor.Models;

namespace TerribleSettingsAuditor.Helpers;

/// <summary>
///  Writes the screening report as JSON (tsa-report.json) for CI/CD pipelines.
/// </summary>
internal static class JsonReport
{
    public const string FileName = "tsa-report.json";

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        WriteIndented = true,

        // ScreeningReport.Configuration and ConfigurationReport.Properties are fields
        IncludeFields = true,

        // keep masked values (i.e., "Server***") readable
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Serialize(ScreeningReport screeningReport)
    {
        return JsonSerializer.Serialize(screeningReport, _options);
    }

    /// <summary>
    ///  Writes the report to tsa-report.json in the current directory.
    /// </summary>
    /// <returns>Full path of the written report.</returns>
    public static string Write(ScreeningReport screeningReport)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), FileName);
        File.WriteAllText(path, Serialize(screeningReport), new UTF8Encoding(false));
        return path;
    }
}
