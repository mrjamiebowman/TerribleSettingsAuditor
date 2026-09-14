using TerribleSettingsAuditor.Core.Helpers;
using TerribleSettingsAuditor.Core.Interfaces;
using TerribleSettingsAuditor.Core.Models;

namespace TerribleSettingsAuditor.Core.Services;

public class TsaConsoleTableWriter : ITsaConsoleTableWriter
{
    public TsaConsoleTableWriter()
    {

    }

    private string[] Headers = new[] { "Luggage Item", "Secret", "Expose", "State" };

    public void WriteConfigTable(List<ReportItem> reportItems)
    {
        if (Headers == null || Headers.Length == 0) {
            throw new ArgumentException("Headers are required.", nameof(Headers));
        }

        // flatten values
        var flattenedItems = reportItems.Select(item => new string[]
        {
            item.Name ?? string.Empty,
            item.Secret ?? string.Empty,
            item.Expose ?? string.Empty,
            item.State ?? string.Empty
        }).ToList();

        // column widths
        int[] widths = GetColumnWidths(Headers, flattenedItems);

        WriteBorder(widths);
        WriteRow(Headers, widths);
        WriteBorder(widths);

        foreach (var row in reportItems)
        {
            var flattenedRow = new string[]
            {
                row.Name ?? string.Empty,
                row.Secret ?? string.Empty,
                row.Expose ?? string.Empty,
                row.State ?? string.Empty
            };

            WriteRow(flattenedRow, widths, row.Pass);

            if (!String.IsNullOrWhiteSpace(row.Message))
            {
                WriteFullRow(row.Message ?? string.Empty, row.Pass);
            }
        }

        WriteBorder(widths);
    }

    private static int[] GetColumnWidths(string[] headers, List<string[]> rows)
    {
        int[] widths = new int[headers.Length];

        for (int i = 0; i < headers.Length; i++)
        {
            widths[i] = headers[i]?.Length ?? 0;
        }

        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length; i++)
            {
                widths[i] = Math.Max(widths[i], row[i]?.Length ?? 0);
            }
        }

        for (int i = 0; i < widths.Length; i++)
        {
            widths[i] += 2;
        }

        return widths;
    }

    private static void WriteBorder(int[] widths)
    {
        CLI.WriteGreen("+");

        foreach (int width in widths)
        {
            CLI.WriteGreen(new string('-', width));
            CLI.WriteGreen("+");
        }

        Console.WriteLine();
    }

    private static void WriteFullRow(string message, bool pass = true)
    {
        var width = 120;

        try
        {
            width = Console.WindowWidth;
        }
        catch (IOException)
        {
            // No interactive console available; use a default width.
        }

        var padding = Math.Max(0, width - message.Length - 3);

        if (pass)
        {
            CLI.WriteGreen("| " + message);
            CLI.WriteGreen(new string(' ', padding) + "|");
        }
        else
        {
            CLI.WriteRed("| " + message);
            CLI.WriteRed(new string(' ', padding) + "|");
        }

        Console.WriteLine();
    }

    private static void WriteRow(string[] values, int[] widths, bool pass = true)
    {
        if (pass == false)
        {
            CLI.WriteRed("|");
        }
        else
        {
            CLI.WriteGreen("|");
        }

        for (int i = 0; i < values.Length; i++)
        {
            string value = values[i] ?? string.Empty;

            if (pass == false)
            {
                CLI.WriteRed(" " + value.PadRight(widths[i] - 1));
                CLI.WriteRed("|");
            }
            else
            {
                CLI.WriteGreen(" " + value.PadRight(widths[i] - 1));
                CLI.WriteGreen("|");
            }
        }

        Console.WriteLine();
    }
}