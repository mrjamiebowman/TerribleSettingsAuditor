namespace TerribleSettingsAuditor.Settings;

/// <summary>
///  Terrible Settings Auditor (TSA) Settings.
///  These override the configuration settings.
/// </summary>
public class TsaSettings
{
    public FeaturesSettings Features = new FeaturesSettings();

    public class FeaturesSettings
    {
        public bool Arguments = true;

        public bool Validation = true;
    }
}
