using Trees.LoadTests.Models;

namespace Trees.LoadTests.Services;

public static class SummaryBuilder
{
    public static IReadOnlyList<CaseSummaryRecord> BuildCaseSummaries(IReadOnlyList<RunRecord> records)
    {
        return records
            .GroupBy(record => new { record.Size, record.Shape, record.Algorithm, record.Scenario })
            .Select(group => new CaseSummaryRecord
            {
                Size = group.Key.Size,
                Shape = group.Key.Shape,
                Algorithm = group.Key.Algorithm,
                Scenario = group.Key.Scenario,
                SampleCount = group.Count(),
                MeanMilliseconds = group.Average(record => record.ElapsedMilliseconds),
                MedianMilliseconds = Median(group.Select(record => record.ElapsedMilliseconds)),
                StdDevMilliseconds = StdDev(group.Select(record => record.ElapsedMilliseconds)),
                MinMilliseconds = group.Min(record => record.ElapsedMilliseconds),
                MaxMilliseconds = group.Max(record => record.ElapsedMilliseconds),
                MeanAllocatedBytes = group.Average(record => (double)record.AllocatedBytes),
                MinAllocatedBytes = group.Min(record => record.AllocatedBytes),
                MaxAllocatedBytes = group.Max(record => record.AllocatedBytes),
                Failures = group.Count(record => !record.MatchesBaseline)
            })
            .OrderBy(record => record.Scenario)
            .ThenBy(record => record.Shape)
            .ThenBy(record => record.Size)
            .ThenBy(record => record.Algorithm)
            .ToList();
    }

    public static IReadOnlyList<AlgorithmSummaryRecord> BuildAlgorithmSummaries(IReadOnlyList<RunRecord> records)
    {
        return records
            .GroupBy(record => new { record.Algorithm, record.Scenario })
            .Select(group => new AlgorithmSummaryRecord
            {
                Algorithm = group.Key.Algorithm,
                Scenario = group.Key.Scenario,
                SampleCount = group.Count(),
                MeanMilliseconds = group.Average(record => record.ElapsedMilliseconds),
                MedianMilliseconds = Median(group.Select(record => record.ElapsedMilliseconds)),
                StdDevMilliseconds = StdDev(group.Select(record => record.ElapsedMilliseconds)),
                MeanAllocatedBytes = group.Average(record => (double)record.AllocatedBytes),
                Failures = group.Count(record => !record.MatchesBaseline)
            })
            .OrderBy(record => record.Scenario)
            .ThenBy(record => record.Algorithm)
            .ToList();
    }

    private static double Median(IEnumerable<double> values)
    {
        var ordered = values.OrderBy(value => value).ToArray();
        if (ordered.Length == 0)
            return 0;

        var middle = ordered.Length / 2;
        return ordered.Length % 2 == 0
            ? (ordered[middle - 1] + ordered[middle]) / 2.0
            : ordered[middle];
    }

    private static double StdDev(IEnumerable<double> values)
    {
        var array = values.ToArray();
        if (array.Length <= 1)
            return 0;

        var mean = array.Average();
        var variance = array.Select(value => Math.Pow(value - mean, 2)).Average();
        return Math.Sqrt(variance);
    }
}
