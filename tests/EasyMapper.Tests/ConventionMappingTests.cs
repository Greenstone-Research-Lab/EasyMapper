namespace EasyMapper.Tests;

public sealed class ConventionMappingTests
{
    [Fact]
    public void Map_WithRuntimeSourceType_CopiesCompatibleProperties()
    {
        object source = new DerivedSource { Name = "Ada", Age = 37, DerivedValue = "visible" };

        Destination result = global::EasyMapper.EasyMapper.Map<Destination>(source);

        Assert.Equal("Ada", result.Name);
        Assert.Equal(37, result.Age);
        Assert.Equal("visible", result.DerivedValue);
    }

    [Fact]
    public void Map_WithCompileTimeTypes_CopiesCompatibleProperties()
    {
        Source source = new() { Name = "Grace", Age = 45 };

        Destination result = global::EasyMapper.EasyMapper.Map<Source, Destination>(source);

        Assert.Equal("Grace", result.Name);
        Assert.Equal(45, result.Age);
    }

    [Fact]
    public void MapTo_UsesTheDefaultMapper()
    {
        Source source = new() { Name = "Linus", Age = 31 };

        Destination result = source.MapTo<Destination>();

        Assert.Equal("Linus", result.Name);
        Assert.Equal(31, result.Age);
    }

    [Fact]
    public void Map_IntoExistingDestination_PreservesUnmatchedProperties()
    {
        Source source = new() { Name = "Margaret", Age = 29 };
        Destination destination = new() { Unmatched = "keep" };

        global::EasyMapper.EasyMapper.Map(source, destination);

        Assert.Equal("Margaret", destination.Name);
        Assert.Equal(29, destination.Age);
        Assert.Equal("keep", destination.Unmatched);
    }

    [Fact]
    public void Mapper_ReusesConventionPlanAcrossCalls()
    {
        Mapper mapper = new();

        Destination first = mapper.Map<Source, Destination>(new Source { Name = "first" });
        Destination second = mapper.Map<Source, Destination>(new Source { Name = "second" });

        Assert.Equal("first", first.Name);
        Assert.Equal("second", second.Name);
    }

    [Fact]
    public void Map_SkipsReadOnlyDestinationsAndIndexers()
    {
        OddSource source = new() { Writable = "mapped", ReadOnly = "ignored" };

        OddDestination result = new Mapper().Map<OddSource, OddDestination>(source);

        Assert.Equal("mapped", result.Writable);
        Assert.Equal("initial", result.ReadOnly);
        Assert.Equal("index", result[0]);
    }

    [Fact]
    public void Map_SkipsPropertiesWithoutPublicAccessors()
    {
        AccessorSource source = new();
        source.SetHidden("hidden");
        source.WriteOnly = "write-only";

        AccessorDestination result = new Mapper().Map<AccessorSource, AccessorDestination>(source);

        Assert.Null(result.Hidden);
        Assert.Null(result.WriteOnly);
        Assert.Equal("unchanged", result.PrivateSetter);
    }

    [Fact]
    public void Map_CaseSensitiveByDefault_DoesNotMatchDifferentCasing()
    {
        LowerCaseSource source = new() { name = "value" };

        Destination result = new Mapper().Map<LowerCaseSource, Destination>(source);

        Assert.Null(result.Name);
    }

    [Fact]
    public void Map_CaseInsensitiveOption_MatchesDifferentCasing()
    {
        Mapper mapper = new(new MapperOptions { CaseSensitive = false });

        Destination result = mapper.Map<LowerCaseSource, Destination>(new LowerCaseSource { name = "value" });

        Assert.Equal("value", result.Name);
    }

    [Fact]
    public void Map_IgnoreNullValues_PreservesExistingValue()
    {
        Mapper mapper = new(new MapperOptions { IgnoreNullValues = true });
        Destination destination = new() { Name = "existing" };

        mapper.Map(new Source { Name = null }, destination);

        Assert.Equal("existing", destination.Name);
    }

    [Fact]
    public void Map_DefaultNullPolicy_AssignsNull()
    {
        Destination destination = new() { Name = "existing" };

        new Mapper().Map(new Source { Name = null }, destination);

        Assert.Null(destination.Name);
    }

    [Fact]
    public async Task Mapper_IsSafeForConcurrentPlanCreationAndExecution()
    {
        Mapper mapper = new();
        Task<Destination>[] tasks = Enumerable.Range(0, 128)
            .Select(index => Task.Run(() => mapper.Map<Source, Destination>(new Source
            {
                Name = index.ToString(),
                Age = index,
            })))
            .ToArray();

        Destination[] results = await Task.WhenAll(tasks);

        Assert.Equal(128, results.Length);
        Assert.All(results, result => Assert.Equal(result.Age.ToString(), result.Name));
    }

    public class Source
    {
        public string? Name { get; set; }

        public int Age { get; set; }
    }

    public sealed class DerivedSource : Source
    {
        public string? DerivedValue { get; set; }
    }

    public sealed class Destination
    {
        public string? Name { get; set; }

        public int Age { get; set; }

        public string? DerivedValue { get; set; }

        public string? Unmatched { get; set; }
    }

    public sealed class LowerCaseSource
    {
#pragma warning disable IDE1006
        public string? name { get; set; }
#pragma warning restore IDE1006
    }

    public sealed class OddSource
    {
        public string? Writable { get; set; }

        public string? ReadOnly { get; set; }

        public string this[int index] => index.ToString();
    }

    public sealed class OddDestination
    {
        public string? Writable { get; set; }

        public string ReadOnly => "initial";

        public string this[int index]
        {
            get => "index";
            set => _ = value;
        }
    }

    public sealed class AccessorSource
    {
        public string? Hidden { private get; set; }

        public string? WriteOnly
        {
            set => _ = value;
        }

        public string PrivateSetter { get; set; } = "source";

        public void SetHidden(string value) => Hidden = value;
    }

    public sealed class AccessorDestination
    {
        public string? Hidden { get; set; }

        public string? WriteOnly { get; set; }

        public string PrivateSetter { get; private set; } = "unchanged";
    }
}
