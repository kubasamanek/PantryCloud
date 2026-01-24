namespace PantryCloud.SharedKernel.Configuration;

/// <summary>
/// Base class for API configuration. Services can extend this to add service-specific configuration.
/// </summary>
public abstract class ApiConfigurationBase
{
    /// <summary>
    /// Gets or sets the connection strings configuration.
    /// </summary>
    public ConnectionStrings ConnectionStrings { get; set; } = new();
    
    /// <summary>
    /// Gets or sets a value indicating whether the application is running in development mode.
    /// </summary>
    public bool IsDevelopment { get; set; }
}

/// <summary>
/// Configuration for database connection strings.
/// </summary>
public class ConnectionStrings
{
    /// <summary>
    /// Gets or sets the default database connection string.
    /// </summary>
    public string DefaultConnection { get; set; } = string.Empty;
}

