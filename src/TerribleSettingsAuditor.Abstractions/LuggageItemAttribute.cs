using System;

namespace TerribleSettingsAuditor.Abstractions;

#nullable enable

[AttributeUsage(AttributeTargets.Property)]
public class LuggageItemAttribute : System.Attribute
{
    public string? Description { get; set; }

    /// <summary>
    ///  Is this a secret? Connection Strings? API Keys?
    /// </summary>
    public bool Secret { get; set; }

    /// <summary>
    ///  If this is set to true, it will expose the value on the report.
    ///  If the value happens to be a secret it will be masked and truncated while
    ///  showing the first and last 4 characters. This is based on a scale.
    /// </summary>
    public ExposeMethod Expose { get; set; }

    /// <summary>
    /// Number of characters to show from the left when masking.
    /// </summary>
    public int ShowLeft { get; set; } = 0;

    /// <summary>
    /// Number of characters to show from the right when masking.
    /// </summary>
    public int ShowRight { get; set; } = 0;

    public LuggageItemAttribute(string? description = null)
    {
        Description = description;
    }
}

public enum ExposeMethod
{
    /// <summary>
    ///  By default we will not expose values in the report to avoid leaking secrets.
    /// </summary>
    None = 0,

    /// <summary>
    ///  This will show either X characters from the left, right or both sides of the value based on the ShowLeft and ShowRight properties. The rest of the value will be masked with asterisks.
    /// </summary>
    Padded = 1,

    /// <summary>
    ///  This exposes the entire value. We do not allow this on secrets for security.
    /// </summary>
    Full = 2
}

#nullable disable
