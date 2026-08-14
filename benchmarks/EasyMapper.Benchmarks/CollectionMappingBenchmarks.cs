using BenchmarkDotNet.Attributes;

namespace EasyMapper.Benchmarks;

/// <summary>
/// Measures repeated element mapping without attributing collection orchestration to one mapper.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[JsonExporterAttribute.Full]
public class CollectionMappingBenchmarks : MapperBenchmarkBase
{
    private MappingSource[] _sources = null!;

    [Params(10, 100, 1_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        InitializeConventionMappers();
        _sources = Enumerable.Range(1, Count).Select(BenchmarkData.CreateSource).ToArray();
    }

    [Benchmark(Baseline = true)]
    public long Manual()
    {
        long checksum = 0;
        foreach (MappingSource source in _sources)
        {
            checksum += BenchmarkData.MapManually(source).Sequence;
        }

        return checksum;
    }

    [Benchmark]
    public long EasyMapper_CachedPlan()
    {
        long checksum = 0;
        foreach (MappingSource source in _sources)
        {
            checksum += EasyMapper.Map<MappingSource, MappingDestination>(source).Sequence;
        }

        return checksum;
    }

    [Benchmark]
    public long AutoMapper_CachedPlan()
    {
        long checksum = 0;
        foreach (MappingSource source in _sources)
        {
            checksum += AutoMapper.Map<MappingDestination>(source).Sequence;
        }

        return checksum;
    }
}
