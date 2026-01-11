namespace PantryCloud.SharedKernel.Configuration;

public abstract class ApiConfigurationBase
{
    public ConnectionStrings ConnectionStrings { get; set; } = new();
    public bool IsDevelopment { get; set; }
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; } = string.Empty;
}

