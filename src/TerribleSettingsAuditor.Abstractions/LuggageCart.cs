using System;

namespace TerribleSettingsAuditor.Abstractions;

/// <summary>
///  BagggeCart, is used to generate configuration files.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class LuggageCart : System.Attribute
{
    public string Name { get; set; }

    public string DefaultFileName { get; set; } = "appsettings.tsa.json";

    public LuggageCart(string name)
    {
        Name = name;
    }

    public LuggageCart(string name, string fileName)
    {
        Name = name;
        DefaultFileName = fileName;
    }
}
