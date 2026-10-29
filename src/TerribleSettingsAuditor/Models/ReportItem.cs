namespace TerribleSettingsAuditor.Models;

public class ReportItem
{
    public string? Icon { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; } 

    public string? Message { get; set; }

    public string? Secret { get; set; }

    public string? Expose { get; set; }

    public bool? Required { get; set; }

    public bool Pass { get; set; }

    public string? State { get; set; }
}
