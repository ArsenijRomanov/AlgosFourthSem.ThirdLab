using Trees.LoadTests.Configuration;
using Trees.LoadTests.Services;

namespace Trees.LoadTests;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var projectRootResolver = new ProjectRootResolver();
            var projectRoot = projectRootResolver.ResolveProjectRoot();

            var configPath = CliArguments.GetConfigPath(args, projectRoot);
            var config = ConfigLoader.Load(configPath);

            var runner = new LoadTestRunner(projectRoot);
            var runDirectory = runner.Run(config, configPath);

            Console.WriteLine($"Results saved to: {runDirectory}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
