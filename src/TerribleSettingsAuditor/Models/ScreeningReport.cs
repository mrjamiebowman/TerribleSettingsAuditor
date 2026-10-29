namespace TerribleSettingsAuditor.Models;

#nullable enable

public class ScreeningReport
{
    public bool Pass { get; set; }

    public List<ConfigurationReport> Configuration = new();
}

public class ConfigurationReport
{
    public bool Passed { get; set; }

    public bool Pinned { get; set; }

    public int? Order { get; set; }

    public string? Name { get; set; }

    public string? Namespace { get; set; }

    public Type? ConfigurationType { get; set; }

    public string? Message { get; set; }

    public List<ConfigurationPropertyReport> Properties = new();
}

public class ConfigurationPropertyReport
{
    public bool BaggageItem { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool Pass { get; set; }

    public string? Message { get; set; }

    public bool? Required { get; set; }

    public int? Order { get; set; } = int.MaxValue;
    
    public bool? Secret { get; set; }

    public bool? Expose { get; set; }

    public string? ExposeValue { get; set; }

    public string? Reason { get; set; }
}

#nullable disable