using System;

namespace TerribleSettingsAuditor.Abstractions;

[AttributeUsage(AttributeTargets.Property)]
public class LuggageItemConnectionStringAttribute : Attribute
{
    public ConnectionStringEnum ConnectionStringType { get; set; }

    public LuggageItemConnectionStringAttribute(ConnectionStringEnum connectionStringType = ConnectionStringEnum.SqlServer)
    {
        ConnectionStringType = connectionStringType;
    }
}
