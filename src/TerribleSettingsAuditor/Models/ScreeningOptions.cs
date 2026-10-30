using System.Reflection;

namespace TerribleSettingsAuditor.Models;

#nullable enable

public class ScreeningOptions
{
    public bool ValidateConfiguration = true;

    public bool NoJoke { get; set; }

    public bool Quiet { get; set; }

    public bool NoAbort { get; set; }

    public bool Json { get; set; }

    public Assembly[] Assemblies { get; set; }
}

#nullable disable