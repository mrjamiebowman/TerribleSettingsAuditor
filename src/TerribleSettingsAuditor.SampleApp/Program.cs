using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using TerribleSettingsAuditor.Core;
using TerribleSettingsAuditor.SampleApp.Domain.Configuration;
using TerribleSettingsAuditor.SampleApp.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

/****************************************/
/*            configuration             */
/****************************************/

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.overrides.json", optional: true, reloadOnChange: true);

////// this is one way to validate configuration but it only validates on run...
////// if we want to see this in a CI/CD pipeline then TSA is the way to go!
//builder.Services
//    .AddOptions<ApplicationOptions>()
//    .Bind(builder.Configuration.GetSection(ApplicationOptions.Position))
//    .ValidateDataAnnotations()
//    .ValidateOnStart()
//;

//builder.Services
//    .AddOptions<BadConfiguration>()
//    .Bind(builder.Configuration.GetSection(BadConfiguration.Position))
//    .ValidateDataAnnotations()
//    .ValidateOnStart();

/****************************************/
/*     tsa (future approach)            */
/****************************************/

//builder.Services
//    .AddOptions<ApplicationOptions>()
//    .Bind(builder.Configuration.GetSection(ApplicationOptions.Position))
//    .ValidateDataAnnotations()
//    .ValidateWithTsa();

//builder.Services
//    .AddOptions<BadConfiguration>()
//    .Bind(builder.Configuration.GetSection(BadConfiguration.Position))
//    .ValidateDataAnnotations()
//    .ValidateWithTsa();

/****************************************/
/*                tsa                   */
/****************************************/

builder.Services.Configure<AzureAppConfigOptions>(builder.Configuration.GetSection(AzureAppConfigOptions.Position));

// configuration (singleton)
DatabaseConfiguration databaseConfiguration = new DatabaseConfiguration();
builder.Configuration.GetSection(DatabaseConfiguration.Position).Bind(databaseConfiguration);
builder.Services.AddSingleton(databaseConfiguration);

// configuration (ioptions)
builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection(ApplicationOptions.Position));

// configuration (snapshot)
builder.Services.Configure<BadConfiguration>(builder.Configuration.GetSection(BadConfiguration.Position));

/****************************************/
/*            health checks             */
/****************************************/

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "sql",
        new SqlConnectionHealthCheck(databaseConfiguration.ConnectionStringSampleApp, timeout: TimeSpan.FromSeconds(3)),
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "tsa", "db", "ready" });

/****************************************/
/*                tsa                   */
/****************************************/

builder.AddTerribleSettingsAuditor();

var app = builder.Build();

/****************************************/
/*                tsa                   */
/****************************************/

// uses the pipeline to validate settings and show the results in a screen if there are any issues.
// If there are issues and AbortScreenFailure is true then the app will exit with a non-zero exit code.
// If there are no issues or if ScreenOnStartup is false then this will do nothing.
// CLI commands always win over settings.
await app.UseTerribleSettingsAuditorAsync(args);

// health checks
app.MapHealthChecks("/health");

// scalar
app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

try
{
    await app.RunAsync();
} catch (Exception)
{
    throw;
}

public partial class Program
{

}