using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Logging.Abstractions;
using EasyMapperInstance = global::EasyMapper.Mapper;

namespace EasyMapper.Benchmarks;

/// <summary>
/// Measures the first usable map, including mapper construction and initial plan compilation.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[SimpleJob(RuntimeMoniker.Net10_0, invocationCount: 1, iterationCount: 15, warmupCount: 5)]
[JsonExporterAttribute.Full]
public class ColdStartBenchmarks
{
    private MappingSource _source = null!;

    [GlobalSetup]
    public void Setup() => _source = BenchmarkData.CreateSource();

    [Benchmark(Baseline = true)]
    public MappingDestination Manual() => BenchmarkData.MapManually(_source);

    [Benchmark]
    public MappingDestination EasyMapper_FirstMap()
    {
        EasyMapperInstance mapper = new();
        return mapper.Map<MappingSource, MappingDestination>(_source);
    }

    [Benchmark]
    public MappingDestination AutoMapper_FirstMap()
    {
        MapperConfiguration configuration = new(
            expression => expression.CreateMap<MappingSource, MappingDestination>(),
            NullLoggerFactory.Instance);
        AutoMapper.IMapper mapper = configuration.CreateMapper();
        return mapper.Map<MappingDestination>(_source);
    }
}
