using System.Text.Json;

namespace Trees.LoadTests.Configuration;

public static class ConfigLoader
{
    public static LoadTestConfig Load(string configPath)
    {
        if (!File.Exists(configPath))
            throw new FileNotFoundException("Configuration file was not found.", configPath);

        var json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<LoadTestConfig>(json, JsonOptions())
            ?? throw new InvalidOperationException("Failed to deserialize configuration.");

        Validate(config, configPath);
        return config;
    }

    public static JsonSerializerOptions JsonOptions()
        => new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

    private static void Validate(LoadTestConfig config, string configPath)
    {
        if (string.IsNullOrWhiteSpace(config.OutputDirectory))
            throw new InvalidOperationException($"OutputDirectory must be set in {configPath}.");

        if (string.IsNullOrWhiteSpace(config.RunFolderPrefix))
            throw new InvalidOperationException($"RunFolderPrefix must be set in {configPath}.");

        if (config.IterationsPerCase <= 0)
            throw new InvalidOperationException($"IterationsPerCase must be greater than 0 in {configPath}.");

        if (config.Sizes.Count == 0 || config.Sizes.Any(size => size <= 0))
            throw new InvalidOperationException($"Sizes must contain positive integers in {configPath}.");

        if (config.TreeShapes.Count == 0)
            throw new InvalidOperationException($"TreeShapes must contain at least one value in {configPath}.");

        if (config.Algorithms.Count == 0)
            throw new InvalidOperationException($"Algorithms must contain at least one value in {configPath}.");
    }
}
