namespace TerribleSettingsAuditor.Core.Models;

/// <summary>
///  Options for running ASP.NET Core health checks from the TSA CLI (i.e., "tsa --health").
/// </summary>
public class HealthScreeningOptions
{
    /// <summary>
    ///  Health check tags to run. Defaults to "tsa". Ignored when <see cref="AllTags"/> is true.
    /// </summary>
    public List<string> Tags { get; set; } = new() { "tsa" };

    /// <summary>
    ///  Run every registered health check regardless of tag.
    /// </summary>
    public bool AllTags { get; set; }

    /// <summary>
    ///  Emit the report as JSON instead of the console table (useful for pipeline assertions).
    /// </summary>
    public bool Json { get; set; }

    /// <summary>
    ///  Treat a "Degraded" result as a failure (exit code 1). Default: Degraded passes.
    /// </summary>
    public bool Strict { get; set; }

    /// <summary>
    ///  Suppress the decorative banner / block header.
    /// </summary>
    public bool Quiet { get; set; }

    /// <summary>
    ///  Print the report but never call Environment.Exit (hand control back to the app).
    /// </summary>
    public bool NoAbort { get; set; }
}
