using AutoMapper;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using AutoMapperInstance = AutoMapper.IMapper;
using EasyMapperInstance = global::EasyMapper.Mapper;

namespace EasyMapper.Benchmarks;

/// <summary>
/// Compares EasyMapper's call-specific lambda API with a preconfigured AutoMapper type map.
/// </summary>
/// <remarks>
/// The method names deliberately expose the different configuration lifetimes. EasyMapper creates
/// an isolated plan for each inline rule, while AutoMapper uses an application-level configuration.
/// </remarks>
[MemoryDiagnoser]
[RankColumn]
[JsonExporterAttribute.Full]
public class ConfiguredMappingBenchmarks
{
    private AutoMapperInstance _autoMapper = null!;
    private EasyMapperInstance _easyMapper = null!;
    private PersonSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        _source = new PersonSource { Id = 42, FirstName = "Ada", LastName = "Lovelace" };
        _easyMapper = new EasyMapperInstance();

        MapperConfiguration configuration = new(
            expression => expression
                .CreateMap<PersonSource, PersonDestination>()
                .ForMember(
                    destination => destination.DisplayName,
                    option => option.MapFrom(source => source.FirstName + " " + source.LastName)),
            NullLoggerFactory.Instance);
        configuration.AssertConfigurationIsValid();
        _autoMapper = configuration.CreateMapper();

        PersonDestination easyDestination = EasyMapper_InlineLambda();
        PersonDestination autoDestination = _autoMapper.Map<PersonDestination>(_source);
        if (easyDestination.Id != autoDestination.Id ||
            !string.Equals(easyDestination.DisplayName, autoDestination.DisplayName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Configured benchmark mappers produced different destinations.");
        }
    }

    [Benchmark(Baseline = true)]
    public PersonDestination Manual() => new()
    {
        Id = _source.Id,
        DisplayName = _source.FirstName + " " + _source.LastName
    };

    [Benchmark]
    public PersonDestination EasyMapper_InlineLambda() =>
        _easyMapper.Map<PersonSource, PersonDestination>(_source, expression => expression
            .Bind(destination => destination.DisplayName)
            .From(source => source.FirstName + " " + source.LastName));

    [Benchmark]
    public PersonDestination AutoMapper_Preconfigured() => _autoMapper.Map<PersonDestination>(_source);
}
