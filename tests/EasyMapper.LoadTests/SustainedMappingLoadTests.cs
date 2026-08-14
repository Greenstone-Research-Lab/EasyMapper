using System.Diagnostics;
using Xunit;

namespace EasyMapper.LoadTests;

public sealed class SustainedMappingLoadTests
{
    [Fact]
    [Trait("Category", "Load")]
    public void CachedPlan_MapsOneMillionObjectsAboveConservativeThroughputFloor()
    {
        const int operationCount = 1_000_000;
        Mapper mapper = new();
        LoadSource source = new() { Id = 42, Name = "load", Amount = 19.95m };

        _ = mapper.Map<LoadSource, LoadDestination>(source);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int index = 0; index < operationCount; index++)
        {
            LoadDestination destination = mapper.Map<LoadSource, LoadDestination>(source);
            Assert.Equal(source.Id, destination.Id);
        }

        stopwatch.Stop();
        double operationsPerSecond = operationCount / stopwatch.Elapsed.TotalSeconds;
        Assert.True(
            operationsPerSecond >= 100_000,
            $"Expected at least 100,000 maps/second but measured {operationsPerSecond:N0}.");
    }

    private sealed class LoadSource
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }

    private sealed class LoadDestination
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }
}
