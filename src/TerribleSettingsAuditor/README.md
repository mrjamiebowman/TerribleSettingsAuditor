# Terrible Settings Auditor (TSA)
Terrible Settings Auditor is an independent developer tool and is not affiliated with or endorsed by the Transportation Security Administration.   
This tool is used for auditing and generating configuration in CI/CD pipelines or on demand configuration testing.   

## Attributes
We combine attributes with DataAnnotations to validate configuration. This tool can also output the configuration value and mask / show some of the secret. This is for linting and quick verification.

### Sample

```csharp
[Luggage("Application settings", Pinned = true, Order = 1)]
public class ApplicationOptions
{
    /// <summary>
    ///  Configuration Key. (i.e., Application:DebugMode)
    /// </summary>
    public const string Position = "Application";

    public bool DebugMode { get; set; } = false;

    [Required]
    [LuggageItem("Application Title", Expose = ExposeMethod.Full)]
    public string? Title { get; set; }

    [LuggageItem("DoesntNeedToBeSet", Expose = ExposeMethod.Full)]
    public bool? DoesntNeedToBeSet { get; set; }
}
```

```csharp
[Luggage("Database Connection strings", Pinned = true)]
public class DatabaseConfiguration
{
    /// <summary>
    ///  Configuration Key. (i.e., Database:ConnectionStringSampleApp)
    /// </summary>
    public const string Position = "Database";
    
    [Required]
    [LuggageItem("SampleApp Connection String", Expose = ExposeMethod.Padded, Secret = true, ShowLeft = 25)]
    public string? ConnectionStringSampleApp { get; set; }
    
    [Required]
    [LuggageItem("UsersDb Connection String", Expose = ExposeMethod.Padded, Secret = true, ShowLeft = 25)]
    public string? ConnectionStringUsersDb { get; set; }
}
```

