namespace TerribleSettingsAuditor.Validators;

public sealed class TsaValidatorRegistration
{
    public Func<object?> Resolver { get; set; }

    public Type OptionsType { get; set; }

    public Type MonitorType { get; set; }

    public string? Name { get; set; }
}
