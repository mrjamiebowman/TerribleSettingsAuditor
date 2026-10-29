using TerribleSettingsAuditor.Models;

namespace TerribleSettingsAuditor.Interfaces;

public interface ITsaConsoleTableWriter
{
    void WriteConfigTable(List<ReportItem> reportItems);
}
