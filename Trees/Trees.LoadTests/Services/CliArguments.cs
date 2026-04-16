namespace Trees.LoadTests.Services;

public static class CliArguments
{
    public static string GetConfigPath(string[] args, string projectRoot)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], "--config", StringComparison.OrdinalIgnoreCase))
                continue;

            if (i + 1 >= args.Length)
                throw new ArgumentException("Missing value after --config.");

            return ResolvePath(args[i + 1], projectRoot);
        }

        return Path.Combine(projectRoot, "appsettings.json");
    }

    private static string ResolvePath(string path, string projectRoot)
        => Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(projectRoot, path));
}
