using TerribleSettingsAuditor.SampleApp.Domain.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

/*************************************/
/*  native configuration validation  */
/*************************************/

builder.Services
    .AddOptions<BadConfiguration>()
    .BindConfiguration(BadConfiguration.Position)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<DatabaseConfiguration>()
    .BindConfiguration(DatabaseConfiguration.Position)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
