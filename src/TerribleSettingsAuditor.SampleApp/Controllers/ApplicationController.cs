using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TerribleSettingsAuditor.SampleApp.Domain.Configuration;

namespace TerribleSettingsAuditor.SampleApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    // logger
    private readonly ILogger<ApplicationController> _logger;

    // configuration
    private readonly DatabaseConfiguration _databaseConfiguration;
    private readonly ApplicationOptions _applicationOptions;
    private readonly BadConfiguration _badConfiguration;

    public ApplicationController(ILogger<ApplicationController> logger, DatabaseConfiguration databaseConfiguration, IOptions<ApplicationOptions> applicationOptions, IOptionsSnapshot<BadConfiguration> badConfiguration)
    {
        _logger = logger;
        _databaseConfiguration = databaseConfiguration;
        _applicationOptions = applicationOptions.Value;
        _badConfiguration = badConfiguration.Value;
    }

    [HttpGet("title")]
    public Task<string> GetApplicationTitle()
    {
        return Task.FromResult(_applicationOptions.Title ?? throw new NullReferenceException($"Settting {nameof(ApplicationOptions)}.{nameof(ApplicationOptions.Title)} not set!"));
    }

    [HttpGet("data")]
    public Task<string> GetApplicationData()
    {
        return Task.FromResult(_databaseConfiguration.ConnectionStringSampleApp != null ? "1..2..3..4." : throw new NullReferenceException($"Settting {nameof(DatabaseConfiguration)}.{nameof(DatabaseConfiguration.ConnectionStringSampleApp)} not set!"));
    }
}
