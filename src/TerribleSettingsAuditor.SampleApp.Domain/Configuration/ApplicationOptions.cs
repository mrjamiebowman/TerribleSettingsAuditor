using System.ComponentModel.DataAnnotations;
using TerribleSettingsAuditor.Abstractions.Attribute;

namespace TerribleSettingsAuditor.SampleApp.Domain.Configuration;

[Luggage("Application settings", Pinned = true, Order = 1)]
public class ApplicationOptions
{
    /// <summary>
    ///  Configuration Key. (i.e., Application:DebugMode)
    /// </summary>
    public const string Position = "Application";

    public bool DebugMode { get; set; } = false;

    [Required]
    [LuggageItem("Application Title", Expose = ExposeMethod.Full)]
    public string? Title { get; set; }

    [LuggageItem("DoesntNeedToBeSet", Expose = ExposeMethod.Full)]
    public bool? DoesntNeedToBeSet { get; set; }
}
