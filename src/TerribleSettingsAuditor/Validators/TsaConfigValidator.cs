using Microsoft.Extensions.Options;
using System.Runtime.ExceptionServices;
using TerribleSettingsAuditor.Interfaces;
using TerribleSettingsAuditor.Models;

namespace TerribleSettingsAuditor.Validators;

public class TsaConfigValidator : ITsaConfigValidator
{
    private readonly TsaValidatorOptions _validatorOptions;

    public TsaConfigValidator(IOptions<TsaValidatorOptions> validators)
    {
        _validatorOptions = validators.Value;
    }

    public Task<List<ConfigurationReport>> ValidateAsync()
    {
        List<ConfigurationReport> configReports = new List<ConfigurationReport>();
        List<Exception>? exceptions = null;

        foreach (TsaValidatorRegistration validatorRegistration in _validatorOptions._validators.Values)
        {
            var configurationReport = new ConfigurationReport();

            try
            {
                // configuration report
                configurationReport.Name = validatorRegistration.Name;
                configurationReport.ConfigurationType = validatorRegistration.MonitorType;

                var resolvedValue = validatorRegistration.Resolver();

                configurationReport.Passed = true;
            }
            catch (OptionsValidationException ex)
            {
                exceptions ??= new();
                exceptions.Add(ex);
            } finally
            {
                configReports.Add(configurationReport);
            }
        }

        return Task.FromResult(configReports);
    }
}
