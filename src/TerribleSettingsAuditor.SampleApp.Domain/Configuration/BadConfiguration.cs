using System.ComponentModel.DataAnnotations;
using TerribleSettingsAuditor.Abstractions;

namespace TerribleSettingsAuditor.SampleApp.Domain.Configuration;

[Luggage("Bad Configuration")]
public class BadConfiguration
{
    /// <summary>
    ///  Configuration Key
    /// </summary>
    public const string Position = "Bad";

    /// <summary>
    ///  This should fail.
    /// </summary>
    [Required]
    [LuggageItem("Bad Setting (Should fail)")]
    public bool? Baddie { get; set; }
}
