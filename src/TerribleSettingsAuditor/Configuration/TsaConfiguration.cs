namespace TerribleSettingsAuditor.Configuration;

public class TsaConfiguration
{
    public const string Position = "TSA";

    /// <summary>
    ///  This option screens configuration on Startup.
    /// </summary>
    public bool ScreenOnStartup { get; set; } = false;

    /// <summary>
    ///  Abort on Screening fails on Startup 
    ///  Default: false
    /// </summary>
    public bool AbortOnScreenFailure { get; set; }

    /// <summary>
    ///  Max length of a masked (Padded) value on the report.
    ///  i.e, "asdfdf***********************************************************2143"
    ///  Default: 30, null or 0 disables the limit.
    /// </summary>
    public int? MaxMaskedValueLength { get; set; } = 30;

    /// <summary>
    ///  Max length of a fully exposed (Full) value on the report. Longer values are truncated with "...".
    ///  Default: 50, null or 0 disables the limit.
    /// </summary>
    public int? MaxFullValueLength { get; set; } = 50;
}
