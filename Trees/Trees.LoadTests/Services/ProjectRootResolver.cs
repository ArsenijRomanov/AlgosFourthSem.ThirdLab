namespace Trees.LoadTests.Services;

public sealed class ProjectRootResolver
{
    private const string ProjectFileName = "Trees.LoadTests.csproj";

    public string ResolveProjectRoot()
    {
        var candidates = new[]
        {
            AppContext.BaseDirectory,
            Directory.GetCurrentDirectory()
        };

        foreach (var candidate in candidates)
        {
            var root = TryFindProjectRoot(candidate);
            if (root is not null)
                return root;
        }

        throw new DirectoryNotFoundException($"Could not locate {ProjectFileName} by walking up from base directory or current directory.");
    }

    private static string? TryFindProjectRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory is not null)
        {
            var projectFilePath = Path.Combine(directory.FullName, ProjectFileName);
            if (File.Exists(projectFilePath))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }
}
