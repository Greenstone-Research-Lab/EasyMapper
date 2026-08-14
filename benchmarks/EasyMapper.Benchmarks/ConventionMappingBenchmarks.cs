using BenchmarkDotNet.Attributes;

namespace EasyMapper.Benchmarks;

/// <summary>
/// Compares warmed convention-plan execution against AutoMapper and direct assignments.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[JsonExporterAttribute.Full]
public class ConventionMappingBenchmarks : MapperBenchmarkBase
{
    [GlobalSetup]
    public void Setup() => InitializeConventionMappers();

    [Benchmark(Baseline = true)]
    public MappingDestination Manual() => BenchmarkData.MapManually(Source);

    [Benchmark]
    public MappingDestination EasyMapper_CachedPlan() =>
        EasyMapper.Map<MappingSource, MappingDestination>(Source);

    [Benchmark]
    public MappingDestination AutoMapper_CachedPlan() => AutoMapper.Map<MappingDestination>(Source);
}
