using TerribleSettingsAuditor.Abstractions;
using TerribleSettingsAuditor.SampleApp.Domain.Configuration;

namespace TerribleSettingsAuditor.SampleApp.Domain;

[LuggageCart("appsettings")]
public class ConfigurationSchema
{
    public ApplicationOptions? ApplicationOptions = new ApplicationOptions()
    {
        Title = "Application Title",
    };

    public BadConfiguration? BadConfiguration { get; set; }

    public DatabaseConfiguration? DatabaseConfiguration = new()
    {
        ConnectionStringSampleApp = "Server=localhost;Database=SampleAppDb;User Id=sa;Password=your_password;",
        ConnectionStringUsersDb = "Server=localhost;Database=UsersDb;User Id=sa;Password=your_password;"
    };
}
