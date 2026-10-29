using System.Reflection;
using TerribleSettingsAuditor.Models;

namespace TerribleSettingsAuditor.Interfaces;

public interface ITSA
{
    Task<ScreeningReport> ScreenAsync(IServiceProvider serviceProvider, ScreeningOptions? screeningOptions, CancellationToken cancellationToken = default);

    Task<List<ConfigurationEntry>> GetConfigurationsAsync(IServiceProvider serviceProvider, Assembly[] assemblies, CancellationToken cancellationToken = default);

    Task<ScreeningReport> ValidateAsync(IServiceProvider serviceProvider, ScreeningOptions? screeningOptions, CancellationToken cancellationToken = default);
}
