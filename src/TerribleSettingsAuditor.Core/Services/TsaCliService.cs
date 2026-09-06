using Microsoft.Extensions.Logging;
using TerribleSettingsAuditor.Core.Configuration;
using TerribleSettingsAuditor.Core.Helpers;
using TerribleSettingsAuditor.Core.Interfaces;
using TerribleSettingsAuditor.Core.Models;

namespace TerribleSettingsAuditor.Core.Services;

public class TsaCliService : ITsaCliService 
{
    // logger
    private readonly ILogger<ITsaCliService> _logger;

    // services
    private readonly ITsaConsoleTableWriter _consoleTableWriter;

    // configuration
    private readonly TsaConfiguration _tsaConfiguration;

    public TsaCliService(ILogger<ITsaCliService> logger, ITsaConsoleTableWriter consoleTableWriter, TsaConfiguration tsaConfiguration)
    {
        _logger = logger;
        _consoleTableWriter = consoleTableWriter;
        _tsaConfiguration = tsaConfiguration;
    }

    public void ShowReport(ScreeningReport screeningReport, ScreeningOptions? screeningOptions)
    {
        Console.WriteLine("");
        if (screeningOptions?.Quiet == false)
        {
            Console.WriteLine("");
            CLI.WriteLineGreen($" 👮 TSA: {CLI.GenerateRandomScreeningReportMessage()}");
            Console.WriteLine("");
        }
        ShowBlock(" 📄 Screening Report");
        Console.WriteLine("");

        // sort & order
        var reports = screeningReport.Configuration.OrderBy(x => x.Pinned == true)
                                                   .ThenBy(x => x.Order).ToList();

        foreach (var item in reports)
        {
            CLI.WriteLinLineYellow($"{CLI.Icons.Luggage} Luggage: {item.Name}, ({item.Namespace})");

            List<ReportItem> reportItems = new List<ReportItem>();

            foreach (var prop in item.Properties.OrderBy(x => x.Name))
            {
                // vars
                var icon = prop.Pass ? CLI.Icons.Success : CLI.Icons.Failure;
                var required = (prop.Required ?? false) ? "Yes" : "";

                // Luggage Item (✅ / ❌)
                var reportItem = new ReportItem();
                reportItem.Name = prop.Name;
                reportItem.Description = prop.Description;
                reportItem.Message = prop.Message;
                reportItem.Icon = icon;
                reportItem.Description = prop.Description;
                reportItem.Secret = (prop.Secret ?? false) ? "Yes" : "";
                reportItem.State = (prop.Pass ? "PASS" : "FAIL");
                reportItem.Pass = prop.Pass;

                // expose
                if (prop.Expose == true)
                {
                    reportItem.Expose = prop.ExposeValue;
                }

                reportItems.Add(reportItem);
            }

            // output config table
            _consoleTableWriter.WriteConfigTable(reportItems);

            Console.WriteLine("");
        }
    }

    public static void ShowLogo()
    {
        CLI.WriteLineGreen(@"");
        CLI.WriteLineGreen(@"              ______");
        CLI.WriteLineGreen(@"             _\  _~-\___");
        CLI.WriteLineGreen(@"    =  = == (____    ___D");
        CLI.WriteLineGreen(@"                \_____\___________________,-~~~~~~~`-.._");
        CLI.WriteLineGreen(@"                / o O o o o o O O o o o o o o O o       |\_");
        CLI.WriteLineGreen(@"                `~-.__        ___..----..                  )");
        CLI.WriteLineGreen(@"                      `---~~\___________ / ------------`````");
        CLI.WriteLineGreen(@"                      =  === (_________D");
        CLI.WriteLineGreen(@"");
    }

    public static void ShowBanner()
    {
        Console.WriteLine("");
        CLI.WriteLineGreen("✈️ Terrible Settings Auditor (TSA)");
        CLI.WriteLineGreen(@"");
        CLI.WriteLineGreen("Terrible Settings Auditor is an independent developer tool and is not affiliated with or endorsed by the Transportation Security Administration.");
        ShowLogo();
        CLI.WriteLinLineYellow("https://github.com/mrjamiebowman/TerribleSettingsAuditor");
        Console.WriteLine("");
    }

    public static void GenerateJoke()
    {
        CLI.WriteLineGreen(@"");
        CLI.WriteLineGreen($"👮 TSA: {CLI.GenerateRandomScreeningReportMessage()}");
        CLI.WriteLineGreen(@"");
    }

    public static void ShowHelp()
    {
        Console.WriteLine("👮 Terrible Settings Auditor (TSA)");
        Console.WriteLine("tsa --help                Show help info");
        Console.WriteLine("tsa --screen              Ensures all configuration and settings are present.");
        Console.WriteLine("tsa --health              Runs health checks tagged \"tsa\" (--tag <name>, --all-tags, --json, --strict).");
        //Console.WriteLine("tsa --generate          Creates a scaffolded tsa.json configuration file for TSA settings.");
        Console.WriteLine("tsa --joke                On the house — courtesy of your flight.");
    }

    public static string CenterText(string text, int width)
    {
        text = text.Trim();

        if (text.Length >= width - 2)
        {
            text = text.Substring(0, width - 4) + "..";
        }

        int padding = (width - text.Length) / 2;

        return new string(' ', padding) + text;
    }

    public static void ShowBlock(string title, string? message = null, int x = 60)
    {
        var strBlock = new string('*', x);

        CLI.WriteLineGreen(strBlock);
        CLI.WriteLineGreen(CenterText(title, x));
        CLI.WriteLineGreen(strBlock);

        if (!String.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("");
            CLI.WriteLineGreen(message);
            Console.WriteLine("");
        }
    }
}
