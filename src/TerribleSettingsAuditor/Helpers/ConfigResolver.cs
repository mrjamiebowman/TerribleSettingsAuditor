namespace TerribleSettingsAuditor.Helpers;

public static class ConfigResolver
{
    public static object? ResolveConfig(IServiceProvider serviceProvider, Type configType)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
        if (configType == null) throw new ArgumentNullException(nameof(configType));

        // 1. Try resolving the config class directly
        var config = serviceProvider.GetService(configType);
        if (config != null)
            return config;

        // 2. Try IOptions<T>.Value
        config = TryGetOptionsValue(
            serviceProvider,
            configType,
            typeof(Microsoft.Extensions.Options.IOptions<>),
            "Value");

        if (config != null)
            return config;

        // 3. Try IOptionsSnapshot<T>.Value
        config = TryGetOptionsValue(
            serviceProvider,
            configType,
            typeof(Microsoft.Extensions.Options.IOptionsSnapshot<>),
            "Value");

        if (config != null)
            return config;

        // 4. Try IOptionsMonitor<T>.CurrentValue
        config = TryGetOptionsValue(
            serviceProvider,
            configType,
            typeof(Microsoft.Extensions.Options.IOptionsMonitor<>),
            "CurrentValue");

        if (config != null)
            return config;

        // 5. Try IOptionsFactory<T>.Create(Options.DefaultName)
        config = TryCreateWithOptionsFactory(serviceProvider, configType);
        if (config != null)
            return config;

        // 6. Last resort: manually apply IConfigureOptions<T>
        config = TryConfigureOptions(serviceProvider, configType);
        if (config != null)
            return config;

        return null;
    }

    private static object? TryGetOptionsValue(
        IServiceProvider serviceProvider,
        Type configType,
        Type openGenericType,
        string propertyName)
    {
        var closedType = openGenericType.MakeGenericType(configType);
        var optionsInstance = serviceProvider.GetService(closedType);
        if (optionsInstance == null)
            return null;

        var property = closedType.GetProperty(propertyName);
        return property?.GetValue(optionsInstance);
    }

    private static object? TryCreateWithOptionsFactory(IServiceProvider serviceProvider, Type configType)
    {
        var factoryType = typeof(Microsoft.Extensions.Options.IOptionsFactory<>).MakeGenericType(configType);
        var factory = serviceProvider.GetService(factoryType);
        if (factory == null)
            return null;

        var createMethod = factoryType.GetMethod("Create", new[] { typeof(string) });
        if (createMethod == null)
            return null;

        // Equivalent to Options.DefaultName
        return createMethod.Invoke(factory, new object[] { string.Empty });
    }

    private static object? TryConfigureOptions(IServiceProvider serviceProvider, Type configType)
    {
        var configureOptionsType = typeof(Microsoft.Extensions.Options.IConfigureOptions<>).MakeGenericType(configType);

        var enumerableType = typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(configureOptionsType);
        var configurators = serviceProvider.GetService(enumerableType) as System.Collections.IEnumerable;

        if (configurators == null)
            return null;

        object? optionsInstance;
        try
        {
            optionsInstance = Activator.CreateInstance(configType);
        }
        catch
        {
            return null;
        }

        if (optionsInstance == null)
            return null;

        var configureMethod = configureOptionsType.GetMethod("Configure", new[] { configType });
        if (configureMethod == null)
            return null;

        var foundAny = false;

        foreach (var configurator in configurators)
        {
            if (configurator == null)
                continue;

            foundAny = true;
            configureMethod.Invoke(configurator, new[] { optionsInstance });
        }

        return foundAny ? optionsInstance : null;
    }
}