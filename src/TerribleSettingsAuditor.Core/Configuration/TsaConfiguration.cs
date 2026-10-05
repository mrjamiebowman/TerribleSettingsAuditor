namespace TerribleSettingsAuditor.Core.Configuration;

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
    ///  This limits how many stars are used when exposing a secret.
    ///  i.e, "asdfdf***********************************************************2143"
    /// </summary>
    public int? DefaultMaxExposeSecretLength { get; set; } = 30;
}
