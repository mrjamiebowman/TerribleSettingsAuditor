using TerribleSettingsAuditor.Models;

namespace TerribleSettingsAuditor.Interfaces;

public interface ITsaCliService
{
    //string CenterText(string text, int width);

    //void GenerateJoke();

    //string GenerateRandomScreeningReportMessage();

    //void ShowBanner();

    //void ShowBlock(string title, string? message = null, int x = 60);

    //void ShowHelp();

    //void ShowLogo();

    void ShowReport(ScreeningReport screeningReport, ScreeningOptions? screeningOptions);

    //void WriteError(string message);

    //void WriteGreen(string message);

    //void WriteRed(string message);

    //void WriteYellow(string message);
}
