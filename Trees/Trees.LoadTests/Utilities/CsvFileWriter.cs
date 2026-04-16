using System.Globalization;
using System.Text;
using Trees.LoadTests.Models;

namespace Trees.LoadTests.Utilities;

public static class CsvFileWriter
{
    public static void WriteRunRecords(string filePath, IEnumerable<RunRecord> records)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            "RunId,StartedAtUtc,Size,Shape,Algorithm,Scenario,Iteration,Seed,ElapsedTicks,ElapsedMilliseconds,AllocatedBytes,Gen0Collections,Gen1Collections,Gen2Collections,MatchesBaseline,ResultDigits,ResultValue,MachineName,FrameworkDescription,OsDescription");

        foreach (var record in records)
        {
            builder.AppendLine(string.Join(',',
                Escape(record.RunId),
                Escape(record.StartedAtUtc.ToString("O", CultureInfo.InvariantCulture)),
                record.Size.ToString(CultureInfo.InvariantCulture),
                Escape(record.Shape.ToString()),
                Escape(record.Algorithm.ToString()),
                Escape(record.Scenario.ToString()),
                record.Iteration.ToString(CultureInfo.InvariantCulture),
                record.Seed.ToString(CultureInfo.InvariantCulture),
                record.ElapsedTicks.ToString(CultureInfo.InvariantCulture),
                record.ElapsedMilliseconds.ToString("G17", CultureInfo.InvariantCulture),
                record.AllocatedBytes.ToString(CultureInfo.InvariantCulture),
                record.Gen0Collections.ToString(CultureInfo.InvariantCulture),
                record.Gen1Collections.ToString(CultureInfo.InvariantCulture),
                record.Gen2Collections.ToString(CultureInfo.InvariantCulture),
                record.MatchesBaseline ? "true" : "false",
                record.ResultDigits.ToString(CultureInfo.InvariantCulture),
                Escape(record.ResultValue),
                Escape(record.MachineName),
                Escape(record.FrameworkDescription),
                Escape(record.OsDescription)));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
    }

    public static void WriteCaseSummaries(string filePath, IEnumerable<CaseSummaryRecord> records)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            "Scenario,Size,Shape,Algorithm,SampleCount,MeanMilliseconds,MedianMilliseconds,StdDevMilliseconds,MinMilliseconds,MaxMilliseconds,MeanAllocatedBytes,Failures");

        foreach (var record in records)
        {
            builder.AppendLine(string.Join(',',
                Escape(record.Scenario.ToString()),
                record.Size.ToString(CultureInfo.InvariantCulture),
                Escape(record.Shape.ToString()),
                Escape(record.Algorithm.ToString()),
                record.SampleCount.ToString(CultureInfo.InvariantCulture),
                record.MeanMilliseconds.ToString("G17", CultureInfo.InvariantCulture),
                record.MedianMilliseconds.ToString("G17", CultureInfo.InvariantCulture),
                record.StdDevMilliseconds.ToString("G17", CultureInfo.InvariantCulture),
                record.MinMilliseconds.ToString("G17", CultureInfo.InvariantCulture),
                record.MaxMilliseconds.ToString("G17", CultureInfo.InvariantCulture),
                record.MeanAllocatedBytes.ToString("G17", CultureInfo.InvariantCulture),
                record.Failures.ToString(CultureInfo.InvariantCulture)));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
    }

    public static void WriteAlgorithmSummaries(string filePath, IEnumerable<AlgorithmSummaryRecord> records)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            "Scenario,Algorithm,SampleCount,MeanMilliseconds,MedianMilliseconds,StdDevMilliseconds,MeanAllocatedBytes,Failures");

        foreach (var record in records)
        {
            builder.AppendLine(string.Join(',',
                Escape(record.Scenario.ToString()),
                Escape(record.Algorithm.ToString()),
                record.SampleCount.ToString(CultureInfo.InvariantCulture),
                record.MeanMilliseconds.ToString("G17", CultureInfo.InvariantCulture),
                record.MedianMilliseconds.ToString("G17", CultureInfo.InvariantCulture),
                record.StdDevMilliseconds.ToString("G17", CultureInfo.InvariantCulture),
                record.MeanAllocatedBytes.ToString("G17", CultureInfo.InvariantCulture),
                record.Failures.ToString(CultureInfo.InvariantCulture)));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}