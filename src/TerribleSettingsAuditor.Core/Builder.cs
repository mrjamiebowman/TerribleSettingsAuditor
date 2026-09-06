using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TerribleSettingsAuditor.Core.Configuration;
using TerribleSettingsAuditor.Core.Helpers;
using TerribleSettingsAuditor.Core.Interfaces;
using TerribleSettingsAuditor.Core.Models;
using TerribleSettingsAuditor.Core.Services;
using TerribleSettingsAuditor.Core.Validators;

namespace TerribleSettingsAuditor.Core;

public static class Builder
{
    private static TsaConfiguration? TsaConfiguration { get; set; }

    /// <summary>
    ///  Add Terrible Settings Auditor (TSA)
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static TBuilder AddTerribleSettingsAuditor<TBuilder>(this TBuilder builder, Action<TsaConfiguration>? tsaSettings = default) where TBuilder : IHostApplicationBuilder
    {
        // configuration
        TsaConfiguration tsaConfiguration = new TsaConfiguration();
        builder.Configuration.GetSection(TsaConfiguration.Position).Bind(tsaConfiguration);
        TsaConfiguration = tsaConfiguration;

        if (tsaSettings != null)
        {
            tsaSettings.Invoke(tsaConfiguration);
        }

        builder.Services.AddSingleton(tsaConfiguration);

        // services
        builder.Services.TryAddTransient<ITsaCliService, TsaCliService>();
        builder.Services.TryAddTransient<ITSA, TSA>();
        builder.Services.TryAddTransient<ITsaConsoleTableWriter, TsaConsoleTableWriter>();
        builder.Services.TryAddTransient<ITsaConfigValidator, TsaConfigValidator>();

        return builder;
    }

    /// <summary>
    ///  Use Terrible Settings Auditor (TSA)
    /// </summary>
    /// <param name="app"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task<IApplicationBuilder> UseTerribleSettingsAuditorAsync(this IApplicationBuilder app, string[]? args = null)
    {
        // tsa configuration
        TsaConfiguration tsaConfiguration = app.ApplicationServices.GetRequiredService<TsaConfiguration>();

        // cts
        CancellationTokenSource cts = new CancellationTokenSource();

        // unicode
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // cli
        await ProcessCliCommandsAsync(app, args, cts.Token);

        // process config
        await ProcessAsync(app, args, cts.Token);

        return app;
    }

    private static async Task ProcessAsync(IApplicationBuilder app, string[]? args = null, CancellationToken cancellationToken = default)
    {
        // configuration
        TsaConfiguration tsaConfiguration = app.ApplicationServices.GetRequiredService<TsaConfiguration>();

        if (tsaConfiguration.ScreenOnStartup == false)
        {
            return;
        }

        // banner
        TsaCliService.ShowBanner();

        // tsa
        var tsa = app.ApplicationServices.GetRequiredService<ITSA>();
        var tsaCli = app.ApplicationServices.GetRequiredService<ITsaCliService>();

        // tsa: screen
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // screening options
        ScreeningOptions screeningOptions = new ScreeningOptions();

        // screening report
        var screeningReport = await tsa.ScreenAsync(app.ApplicationServices, screeningOptions, cancellationToken);

        // render report
        tsaCli.ShowReport(screeningReport, screeningOptions);
        
        // cli: ci/cd
        if (screeningReport.Pass == false && tsaConfiguration.AbortOnScreenFailure == true)
        {
            // fail
            Environment.Exit(1);
        }
    }

    private static async Task ProcessCliCommandsAsync(IApplicationBuilder app, string[]? args = null, CancellationToken cancellationToken = default)
    {
        if (
            (args == null || args.Length == 0 || args?.Contains("tsa") == false))
        {
            // if we aren't validating on startup and we don't have a tsa argument then return...
            return;
        }

        // tsa
        var tsa = app.ApplicationServices.GetRequiredService<ITSA>();
        var tsaCli = app.ApplicationServices.GetRequiredService<ITsaCliService>();

        /****************************************/
        /*                params                */
        /****************************************/

        // vars
        bool optionNoJoke = args
            .Skip(1)
            .Any(x => x.Equals("--no-joke", StringComparison.OrdinalIgnoreCase)
                   || x.Equals("--nojoke", StringComparison.OrdinalIgnoreCase));

        bool optionQuiet = args
            .Skip(1)
            .Any(x => x.Equals("--quiet", StringComparison.OrdinalIgnoreCase));

        bool optionNoAbort = args
            .Skip(1)
            .Any(x => x.Equals("--no-abort", StringComparison.OrdinalIgnoreCase)
                   || x.Equals("--noabort", StringComparison.OrdinalIgnoreCase));

        ScreeningOptions screeningOptions = new ScreeningOptions();
        screeningOptions.NoJoke = optionNoJoke;
        screeningOptions.Quiet = optionQuiet;
        screeningOptions.NoAbort = optionNoAbort;

        /****************************************/
        /*             general cli              */
        /****************************************/

        // --quiet
        if (screeningOptions.Quiet == false)
        {
            // banner
            TsaCliService.ShowBanner();
        }

        // help
        if (args[0] == "tsa" && (args[1] == "--help" || args[1] == "-h"))
        {
            TsaCliService.ShowHelp();
            Environment.Exit(0);
        }

        // tsa: generate config
        if (args[0] == "tsa" && (args[1] == "--generate-config" || args[1] == "-gc"))
        {
            CLI.WriteLinLineYellow("Generating TSA Config...");
            Environment.Exit(0);
        }

        // tsa: joke
        if (args[0] == "tsa" && (args[1] == "--joke" || args[1] == "-j"))
        {
            TsaCliService.GenerateJoke();
            Environment.Exit(0);
        }

        /****************************************/
        /*               screening              */
        /****************************************/

        // tsa: screen
        if (args[0] == "tsa" && (args[1] == "--screen" || args[1] == "-s"))
        {
            screeningOptions.Assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // screening report
            var screeningReport = await tsa.ScreenAsync(app.ApplicationServices, screeningOptions, cancellationToken);

            // render report
            tsaCli.ShowReport(screeningReport, screeningOptions);

            if (screeningOptions.NoAbort)
            {
                // return app or this will stop execution.
                return;
            }

            // will abort (ci/cd)
            if (screeningReport.Pass == true)
            {
                // success
                Environment.Exit(0);
            }
            else
            {
                // fail
                Environment.Exit(1);
            }
        }

        /****************************************/
        /*             health checks            */
        /****************************************/

        // tsa: health
        if (args[0] == "tsa" && (args[1] == "--health" || args[1] == "--health-check" || args[1] == "-hc"))
        {
            var hcArgs = args.Skip(2).ToArray();

            var healthOptions = new HealthScreeningOptions
            {
                AllTags = hcArgs.Any(x => x.Equals("--all-tags", StringComparison.OrdinalIgnoreCase)),
                Json = hcArgs.Any(x => x.Equals("--json", StringComparison.OrdinalIgnoreCase)),
                Strict = hcArgs.Any(x => x.Equals("--strict", StringComparison.OrdinalIgnoreCase)),
                Quiet = optionQuiet,
                NoAbort = optionNoAbort,
            };

            // --tag <name> (repeatable); when omitted the default "tsa" tag is used
            var tags = new List<string>();
            for (int i = 0; i < hcArgs.Length - 1; i++)
            {
                if (hcArgs[i].Equals("--tag", StringComparison.OrdinalIgnoreCase))
                {
                    tags.Add(hcArgs[i + 1]);
                }
            }

            if (tags.Count > 0)
            {
                healthOptions.Tags = tags;
            }

            // renders the report and (unless --no-abort) sets the process exit code
            await tsa.ProcessHealthChecksAsync(app.ApplicationServices, healthOptions, cancellationToken);

            return;
        }

        /****************************************/
        /*               validation             */
        /****************************************/

        //// tsa: validate
        //if (args[0] == "tsa" && (args[1] == "--validate" || args[1] == "-v"))
        //{
        //    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //    var screeningReport = await tsa.ValidateAsync(app.ApplicationServices, assemblies);
        //    TsaCli.ShowReport(screeningReport);

        //    if (tsaConfiguration.AbortValidationFailure == false || screeningReport.Pass == true)
        //    {
        //        Environment.Exit(0);
        //    }
        //    else
        //    {
        //        Environment.Exit(1);
        //    }
        //}
    }

    public static OptionsBuilder<TOptions> ValidateWithTsa<TOptions>(this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));

        optionsBuilder.Services.AddOptions<TsaValidatorOptions>()
            .Configure<IOptionsMonitor<TOptions>>((vo, options) =>
            {
                var tsaRegistration = new TsaValidatorRegistration()
                {
                    Resolver = () => options.Get(optionsBuilder.Name),
                    OptionsType = typeof(TOptions),
                    MonitorType = options.GetType(),
                    Name = optionsBuilder.Name
                };

                vo._validators[(typeof(TOptions), optionsBuilder.Name)] = tsaRegistration;
            });

        return optionsBuilder;
    }
}
