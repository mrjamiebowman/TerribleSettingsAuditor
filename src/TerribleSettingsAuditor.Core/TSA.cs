using Microsoft.Extensions.Logging;
using System.Reflection;
using TerribleSettingsAuditor.Abstractions.Attribute;
using TerribleSettingsAuditor.Core.Configuration;
using TerribleSettingsAuditor.Core.Helpers;
using TerribleSettingsAuditor.Core.Interfaces;
using TerribleSettingsAuditor.Core.Models;

namespace TerribleSettingsAuditor.Core;

public class TSA : ITSA
{
    // logger
    private ILogger<TSA> _logger;

    private ITsaConfigValidator _tsaValidator;

    // configuration
    private readonly TsaConfiguration _tsaConfiguration;

    public TSA(ILogger<TSA> logger, ITsaConfigValidator tsaValidator, TsaConfiguration tsaConfiguration)
    {
        _logger = logger;
        _tsaValidator = tsaValidator;
        _tsaConfiguration = tsaConfiguration;
    }

    public async Task<ScreeningReport> ScreenAsync(IServiceProvider serviceProvider, ScreeningOptions? screeningOptions, CancellationToken cancellationToken = default)
    {
        // result
        var screeningReport = new ScreeningReport();

        bool pass = true;

        // default screening settings
        if (screeningOptions == null)
        {
            screeningOptions = new ScreeningOptions();
        }

        // configurations
        List<ConfigurationEntry> configurations = new List<ConfigurationEntry>();

        // assemblies
        configurations = await GetConfigurationsAsync(serviceProvider, screeningOptions.Assemblies, cancellationToken);

        // process configurations
        foreach (var configKey in configurations)
        {
            /**************************************************/
            /*             carry-on (configuration)           */
            /**************************************************/

            bool configPass = true;

            var luggage = new ConfigurationReport()
            {
                Name = configKey.ClassName,
                Namespace = configKey.Namespace
            };

            // resolve config class
            var config = ConfigResolver.ResolveConfig(serviceProvider, configKey.Type);

            // not found
            if (config == null)
            {
                throw new ArgumentNullException("Configuration class not found.");
            }

            var configType = config?.GetType();

            // luggage
            var carryOnAttr = configType.GetCustomAttribute<LuggageAttribute>();

            // properties
            var properties = configType?.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            /**************************************************/
            /*                   validation                   */
            /**************************************************/

            if (properties != null && properties.Any())
            {
                foreach (var prop in properties)
                {
                    /**************************************************/
                    /*          baggage item (property check)         */
                    /**************************************************/

                    var baggageAttr = prop.GetCustomAttribute<LuggageItemAttribute>();
                    var baggageAttrConnectionString = prop.GetCustomAttribute<LuggageItemConnectionStringAttribute>();

                    /**************************************************/
                    /*                  validation                    */
                    /**************************************************/

                    // validate
                    var result = PropertyValidator.ValidateProperty(config, prop.Name);

                    bool passed = false;

                    if (!result.Any())
                    {
                        passed = true;
                    }
                    else
                    {
                        pass = false;
                        configPass = false;
                    }

                    // message
                    string message = string.Join(", ",
                        result
                            .Select(r => r.ErrorMessage)
                            .Where(m => !string.IsNullOrWhiteSpace(m)));

                    // required
                    var required = PropertyValidator.IsRequired(prop) ? true : false;

                    // secret
                    bool isSecret = baggageAttr?.Secret ?? false;

                    // expose value
                    bool expose = false;
                    string exposeValue = "";

                    // warning
                    if (isSecret == true && baggageAttr?.Expose == ExposeMethod.Full)
                    {
                        _logger.LogWarning("Property {PropertyName} in {ClassName} is marked as Secret but has an Expose method set to {ExposeMethod}. Secrets should not be exposed. Please review the configuration.", prop.Name, configType.Name, baggageAttr.Expose);
                    }

                    // we don't allow secrets to be fully exposed. This is a safety measure to prevent accidental exposure of sensitive information.
                    if (isSecret == false && baggageAttr?.Expose == ExposeMethod.Full)
                    {
                        // expose: full
                        object? rawValue = prop.GetValue(config);
                        exposeValue = rawValue?.ToString() ?? "";
                        expose = true;
                    }
                    else if (baggageAttr?.Expose == ExposeMethod.Padded) 
                    {
                        // expose: padded
                        int? left = baggageAttr.ShowLeft;
                        int? right = baggageAttr.ShowRight;

                        object? rawValue = prop.GetValue(config);
                        string? val = rawValue?.ToString();

                        if (isSecret == true) 
                        {
                            // limit "****" in padding.
                            exposeValue = MaskingHelper.MaskMiddleWithLimits(val, left, right, '*', _tsaConfiguration.DefaultMaxExposeSecretLength);
                        } else
                        {
                            exposeValue = MaskingHelper.MaskMiddle(val, left, right);
                        }
                        
                        expose = true;
                    }

                    // baggage item
                    var baggageItem = new ConfigurationPropertyReport()
                    {
                        BaggageItem = baggageAttr != null ? true : false,
                        Name = prop.Name,
                        Description = baggageAttr?.Description ?? String.Empty,
                        Pass = passed,
                        Message = message,
                        Required = required,
                        Secret = isSecret,
                        Expose = expose,
                        ExposeValue = exposeValue
                    };

                    // baggage item
                    luggage.Properties.Add(baggageItem);
                }
            }

            // map
            luggage.Order = carryOnAttr?.Order;
            luggage.Pinned = carryOnAttr?.Pinned ?? false;
            luggage.Passed = configPass;

            // configuration
            screeningReport.Configuration.Add(luggage);
        }

        // pass or fail
        screeningReport.Pass = pass;

        return screeningReport;
    }

    public async Task<ScreeningReport> ValidateAsync(IServiceProvider serviceProvider, ScreeningOptions? screeningOptions, CancellationToken cancellationToken = default)
    {
        // result
        var screeningReport = new ScreeningReport();

        // default screening settings
        if (screeningOptions == null)
        {
            screeningOptions = new ScreeningOptions();
        }

        // configurations
        List<ConfigurationEntry> configurations = new List<ConfigurationEntry>();

        foreach (var assembly in screeningOptions.Assemblies)
        {
            _logger.LogDebug("Loaded Assembly: {assembly}", assembly.FullName);
        }

        // assemblies
        configurations = await GetConfigurationsAsync(serviceProvider, screeningOptions.Assemblies, cancellationToken);

        // process configurations
        foreach (var config in configurations)
        {
            // validate

            // append
        }

        return screeningReport;
    }

    #region private methods

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }

    public Task<List<ConfigurationEntry>> GetConfigurationsAsync(IServiceProvider serviceProvider, Assembly[] assemblies, CancellationToken cancellationToken = default)
    {
        var configurationEntries = new List<ConfigurationEntry>();

        if (assemblies is null)
        {
            return Task.FromResult(configurationEntries);
        }

        foreach (var assembly in assemblies)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // load types
            var typesWithAttribute = GetLoadableTypes(assembly)
                                        .Where(t => t.IsClass && t.GetCustomAttribute<LuggageAttribute>() != null)
                                        .ToList();

            foreach (var type in typesWithAttribute)
            {
                // You can extract property info, values, metadata, etc., from these types
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var configEntry = new ConfigurationEntry()
                { 
                    Assembly = assembly.FullName,
                    Namespace = type.Namespace,
                    ClassName = type.Name!,
                    Type = type
                };

                foreach (var prop in properties)
                {
                    // You can adjust this to match your ConfigurationEntry needs
                    configEntry.Properties.Add(new ConfigurationProperty
                    {
                        PropertyName = prop.Name,
                        PropertyType = prop.PropertyType.FullName!
                    });
                }

                configurationEntries.Add(configEntry);
            }
        }

        return Task.FromResult(configurationEntries);
    }

    #endregion

    //public async Task<ScreeningReport> ScreenAsync(IServiceProvider serviceProvider, Assembly[] assemblies, Action<ScreeningSettings>? SsreeningSettingsAction = null, CancellationToken cancellationToken = default)
    //{
    //    // result
    //    var screeningReport = new ScreeningReport();

    //    bool pass = true;

    //    // default screening settings
    //    ScreeningSettings screeningSettings = new ScreeningSettings();

    //    if (SsreeningSettingsAction != null)
    //    {
    //        SsreeningSettingsAction.Invoke(screeningSettings);
    //    }

    //    // configurations
    //    List<ConfigurationEntry> configurations = new List<ConfigurationEntry>();

    //    //// validate
    //    //var configReport = await _tsaValidator.ValidateAsync();

    //    //var test = "";

    //    // assemblies
    //    configurations = await GetConfigurationsAsync(serviceProvider, assemblies, cancellationToken);

    //    // process configurations
    //    foreach (var configKey in configurations)
    //    {
    //        /**************************************************/
    //        /*             carry-on (configuration)           */
    //        /**************************************************/

    //        bool configPass = true;

    //        var carryOn = new ConfigurationReport()
    //        {
    //            Name = configKey.ClassName,
    //            Namespace = configKey.Namespace
    //        };

    //        // resolve config class
    //        var config = ConfigResolver.ResolveConfig(serviceProvider, configKey.Type);

    //        // not found
    //        if (config == null)
    //        {
    //            throw new ArgumentNullException("Configuration class not found.");
    //        }

    //        var configType = config?.GetType();

    //        // carry-on
    //        var carryOnAttr = configType.GetCustomAttribute<LuggageAttribute>();

    //        // properties
    //        var properties = configType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

    //        /**************************************************/
    //        /*                   validation                   */
    //        /**************************************************/

    //        foreach (var prop in properties)
    //        {
    //            /**************************************************/
    //            /*          baggage item (property check)         */
    //            /**************************************************/

    //            var baggageAttr = prop.GetCustomAttribute<LuggageItemAttribute>();
    //            var baggageAttrConnectionString = prop.GetCustomAttribute<LuggageItemConnectionStringAttribute>();

    //            /**************************************************/
    //            /*                  validation                    */
    //            /**************************************************/

    //            // validate
    //            var result = PropertyValidator.ValidateProperty(config, prop.Name);

    //            bool passed = false;

    //            if (!result.Any())
    //            {
    //                passed = true;
    //            }
    //            else
    //            {
    //                pass = false;
    //                configPass = false;
    //            }

    //            // message
    //            string message = string.Join(
    //                ", ",
    //                result
    //                    .Select(r => r.ErrorMessage)
    //                    .Where(m => !string.IsNullOrWhiteSpace(m)));

    //            // required
    //            var required = PropertyValidator.IsRequired(prop) ? true : false;

    //            // baggage item
    //            var baggageItem = new ConfigurationPropertyReport()
    //            {
    //                BaggageItem = baggageAttr != null ? true : false,
    //                Name = prop.Name,
    //                Description = baggageAttr?.Description ?? String.Empty,
    //                Pass = passed,
    //                Message = message,
    //                Required = required
    //            };

    //            // baggage item
    //            carryOn.Properties.Add(baggageItem);
    //        }

    //        // pass or fail
    //        carryOn.Passed = configPass;

    //        // configuration
    //        screeningReport.Configuration.Add(carryOn);
    //    }

    //    // pass or fail
    //    screeningReport.Pass = pass;

    //    return screeningReport;
    //}
}
