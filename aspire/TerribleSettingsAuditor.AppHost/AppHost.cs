using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// app profile 
var launchProfile = builder.Configuration.GetValue<string>("LAUNCH_PROFILE") ?? "DEV";

// projects
builder.AddProject<TerribleSettingsAuditor_SampleApp>("api-sampleapp", launchProfile);

builder.AddProject<Projects.TerribleSettingsAuditor_SampleApp_NoTsa>("terriblesettingsauditor-sampleapp-notsa");

builder.Build().Run();
