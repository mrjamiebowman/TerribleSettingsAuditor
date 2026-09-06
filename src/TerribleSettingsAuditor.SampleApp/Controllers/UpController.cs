using Microsoft.AspNetCore.Mvc;

namespace TerribleSettingsAuditor.SampleApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpController : ControllerBase
{
    // logger
    private readonly ILogger<UpController> _logger;

    public UpController(ILogger<UpController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public Task<string> GetApplicationTitle()
    {
        _logger.LogInformation("UP");
        return Task.FromResult("OK");
    }
}
