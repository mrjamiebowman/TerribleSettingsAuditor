using System.Text.Json;
using TerribleSettingsAuditor.Helpers;
using TerribleSettingsAuditor.Models;

namespace TerribleSettingsAuditor.Core.Tests.Unit.Helpers;

public class JsonReportTest
{
    private static ScreeningReport CreateReport()
    {
        var report = new ScreeningReport() { Pass = false };

        var luggage = new ConfigurationReport()
        {
            Name = "DatabaseConfiguration",
            Namespace = "MyApp.Configuration",
            ConfigurationType = typeof(JsonReportTest)
        };

        luggage.Properties.Add(new ConfigurationPropertyReport()
        {
            Name = "ConnectionString",
            Pass = false,
            Message = "The ConnectionString field is required.",
            Secret = true,
            Expose = true,
            ExposeValue = "Server***+True"
        });

        report.Configuration.Add(luggage);

        return report;
    }

    [Fact]
    public void Serialize_IncludesConfigurationAndPropertyFields()
    {
        using var json = JsonDocument.Parse(JsonReport.Serialize(CreateReport()));
        var root = json.RootElement;

        Assert.False(root.GetProperty("Pass").GetBoolean());

        var luggage = root.GetProperty("Configuration")[0];
        Assert.Equal("DatabaseConfiguration", luggage.GetProperty("Name").GetString());

        var item = luggage.GetProperty("Properties")[0];
        Assert.Equal("ConnectionString", item.GetProperty("Name").GetString());
        Assert.Equal("The ConnectionString field is required.", item.GetProperty("Message").GetString());
    }

    [Fact]
    public void Serialize_SkipsConfigurationType()
    {
        using var json = JsonDocument.Parse(JsonReport.Serialize(CreateReport()));

        Assert.False(json.RootElement.GetProperty("Configuration")[0].TryGetProperty("ConfigurationType", out _));
    }

    [Fact]
    public void Serialize_KeepsMaskedValuesReadable()
    {
        Assert.Contains("\"Server***+True\"", JsonReport.Serialize(CreateReport()));
    }

    [Fact]
    public void Write_CreatesReportInCurrentDirectory()
    {
        var previous = Directory.GetCurrentDirectory();
        var dir = Directory.CreateTempSubdirectory("tsa-");

        try
        {
            Directory.SetCurrentDirectory(dir.FullName);

            var path = JsonReport.Write(CreateReport());

            Assert.Equal(Path.Combine(dir.FullName, JsonReport.FileName), path);
            Assert.True(File.Exists(path));
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
            dir.Delete(true);
        }
    }
}
