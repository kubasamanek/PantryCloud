namespace PantryCloud.SharedKernel.Configuration;

public abstract class ApiConfigurationBase
{
    public ConnectionStrings ConnectionStrings { get; set; } = new();
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; } = string.Empty;
}

