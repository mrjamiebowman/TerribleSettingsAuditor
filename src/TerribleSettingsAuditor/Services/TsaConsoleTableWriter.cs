using TerribleSettingsAuditor.Helpers;
using TerribleSettingsAuditor.Interfaces;
using TerribleSettingsAuditor.Models;

namespace TerribleSettingsAuditor.Services;

public class TsaConsoleTableWriter : ITsaConsoleTableWriter
{
    public TsaConsoleTableWriter()
    {

    }

    private string[] Headers = new[] { "Luggage Item", "Secret", "Value", "State" };

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
                WriteFullRow(row.Message ?? string.Empty, widths, row.Pass);
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

    private static void WriteFullRow(string message, int[] widths, bool pass = true)
    {
        // table width minus the outer borders and a space either side ("| " ... " |")
        int tableWidth = widths.Sum() + widths.Length + 1;
        int contentWidth = Math.Max(1, tableWidth - 4);

        foreach (var line in WrapText(message, contentWidth))
        {
            var text = "| " + line.PadRight(contentWidth) + " |";

            if (pass)
            {
                CLI.WriteGreen(text);
            }
            else
            {
                CLI.WriteRed(text);
            }

            Console.WriteLine();
        }
    }

    private static List<string> WrapText(string text, int width)
    {
        var lines = new List<string>();
        var current = string.Empty;

        foreach (var word in text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            // hard split words that are longer than a full line
            var remaining = word;
            while (remaining.Length > width)
            {
                if (current.Length > 0)
                {
                    lines.Add(current);
                    current = string.Empty;
                }

                lines.Add(remaining.Substring(0, width));
                remaining = remaining.Substring(width);
            }

            if (current.Length == 0)
            {
                current = remaining;
            }
            else if (current.Length + 1 + remaining.Length <= width)
            {
                current += " " + remaining;
            }
            else
            {
                lines.Add(current);
                current = remaining;
            }
        }

        if (current.Length > 0 || lines.Count == 0)
        {
            lines.Add(current);
        }

        return lines;
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