using Microsoft.Extensions.Logging;

namespace TerribleSettingsAuditor.Core.Tests.Unit;

public abstract class BaseTsaTest
{
    public ILogger<TSA> GetLogger()
    {
        var factory = LoggerFactory.Create(builder =>
        {
            //builder.AddConsole(); // or AddDebug(), AddEventSourceLogger(), etc.
        });

        ILogger<TSA> logger = factory.CreateLogger<TSA>();

        return logger;
    }
}
