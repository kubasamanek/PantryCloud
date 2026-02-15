namespace PantryCloud.SharedKernel.Testing.Infrastructure.Environment;

/// <summary>
/// Resolves paths relative to the solution root for integration tests.
/// </summary>
public static class SolutionPathHelper
{
    /// <summary>
    /// Returns a path relative to the solution root.
    /// </summary>
    /// <param name="pathSegments">Path segments to combine (e.g. "src", "Services", "PantryCloud.Identity", "PantryCloud.Identity.Presentation", "Secrets")</param>
    public static string GetPathFromSolutionRoot(params string[] pathSegments)
    {
        var solutionDir = GetSolutionDirectory();
        return Path.Combine(solutionDir, Path.Combine(pathSegments));
    }

    /// <summary>
    /// Finds the solution directory by walking up from the current directory until a .sln file is found.
    /// </summary>
    private static string GetSolutionDirectory()
    {
        var baseDir = AppContext.BaseDirectory;
        var dir = new DirectoryInfo(baseDir);
        while (dir != null && dir.GetFiles("*.sln").Length == 0)
        {
            dir = dir.Parent;
        }
        if (dir == null)
            throw new InvalidOperationException("Could not find solution directory. Ensure tests run from within the solution structure.");
        return dir.FullName;
    }
}
