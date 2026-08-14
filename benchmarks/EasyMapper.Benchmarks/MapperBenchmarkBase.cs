using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using AutoMapperInstance = AutoMapper.IMapper;
using EasyMapperInstance = global::EasyMapper.Mapper;

namespace EasyMapper.Benchmarks;

/// <summary>
/// Provides warmed mapper instances and validates equivalent convention mappings before measurement.
/// </summary>
public abstract class MapperBenchmarkBase
{
    protected AutoMapperInstance AutoMapper { get; private set; } = null!;

    protected EasyMapperInstance EasyMapper { get; private set; } = null!;

    protected MappingSource Source { get; private set; } = null!;

    protected void InitializeConventionMappers()
    {
        Source = BenchmarkData.CreateSource();
        EasyMapper = new EasyMapperInstance();

        MapperConfiguration configuration = new(
            expression => expression.CreateMap<MappingSource, MappingDestination>(),
            NullLoggerFactory.Instance);
        configuration.AssertConfigurationIsValid();
        AutoMapper = configuration.CreateMapper();

        MappingDestination easyDestination = EasyMapper.Map<MappingSource, MappingDestination>(Source);
        MappingDestination autoDestination = AutoMapper.Map<MappingDestination>(Source);
        BenchmarkData.ValidateEquivalent(Source, easyDestination);
        BenchmarkData.ValidateEquivalent(Source, autoDestination);
    }
}
