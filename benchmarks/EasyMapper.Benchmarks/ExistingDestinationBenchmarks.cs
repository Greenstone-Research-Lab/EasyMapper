using BenchmarkDotNet.Attributes;

namespace EasyMapper.Benchmarks;

/// <summary>
/// Measures mapping into an existing destination while keeping allocation behavior equivalent.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[JsonExporterAttribute.Full]
public class ExistingDestinationBenchmarks : MapperBenchmarkBase
{
    [GlobalSetup]
    public void Setup() => InitializeConventionMappers();

    [Benchmark(Baseline = true)]
    public MappingDestination Manual()
    {
        MappingDestination destination = new();
        BenchmarkData.MapManually(Source, destination);
        return destination;
    }

    [Benchmark]
    public MappingDestination EasyMapper_CachedPlan()
    {
        MappingDestination destination = new();
        EasyMapper.Map(Source, destination);
        return destination;
    }

    [Benchmark]
    public MappingDestination AutoMapper_CachedPlan()
    {
        MappingDestination destination = new();
        AutoMapper.Map(Source, destination);
        return destination;
    }
}
