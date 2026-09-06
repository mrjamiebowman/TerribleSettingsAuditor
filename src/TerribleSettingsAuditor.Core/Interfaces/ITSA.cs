using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TerribleSettingsAuditor.Core.Models;

namespace TerribleSettingsAuditor.Core.Interfaces;

public interface ITSA
{
    Task<ScreeningReport> ScreenAsync(IServiceProvider serviceProvider, ScreeningOptions? screeningOptions, CancellationToken cancellationToken = default);

    Task<HealthReport> ProcessHealthChecksAsync(IServiceProvider serviceProvider, HealthScreeningOptions? options, CancellationToken cancellationToken = default);

    Task<List<ConfigurationEntry>> GetConfigurationsAsync(IServiceProvider serviceProvider, Assembly[] assemblies, CancellationToken cancellationToken = default);

    Task<ScreeningReport> ValidateAsync(IServiceProvider serviceProvider, ScreeningOptions? screeningOptions, CancellationToken cancellationToken = default);
}
