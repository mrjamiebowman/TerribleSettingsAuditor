using System.ComponentModel.DataAnnotations;
using TerribleSettingsAuditor.Abstractions;

namespace TerribleSettingsAuditor.SampleApp.Domain.Configuration;

[Luggage("Database Connection strings", Pinned = true)]
public class DatabaseConfiguration
{
    /// <summary>
    ///  Configuration Key. (i.e., Database:ConnectionStringSampleApp)
    /// </summary>
    public const string Position = "Database";
    
    [Required]
    [LuggageItem("SampleApp Connection String", Expose = ExposeMethod.Padded, Secret = true, ShowLeft = 25)]
    public string? ConnectionStringSampleApp { get; set; }
    
    [Required]
    [LuggageItem("UsersDb Connection String", Expose = ExposeMethod.Padded, Secret = true, ShowLeft = 25)]
    public string? ConnectionStringUsersDb { get; set; }
}
