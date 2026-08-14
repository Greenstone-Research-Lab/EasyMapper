namespace EasyMapper.Tests;

public sealed class ErrorContractTests
{
    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Mapper(null!));
    }

    [Fact]
    public void RuntimeMap_NullSource_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Mapper().Map<ValidDestination>(null!));
    }

    [Fact]
    public void GenericMap_NullSource_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Mapper().Map<ValidSource, ValidDestination>(null!));
    }

    [Fact]
    public void ConfiguredMap_NullConfiguration_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Mapper().Map<ValidSource, ValidDestination>(
            new ValidSource(),
            (Action<IMappingExpression<ValidSource, ValidDestination>>)null!));
    }

    [Fact]
    public void ExistingMap_NullDestination_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Mapper().Map(new ValidSource(), (ValidDestination)null!));
    }

    [Fact]
    public void ExistingConfiguredMap_NullArguments_ThrowArgumentNullException()
    {
        Mapper mapper = new();
        ValidSource source = new();
        ValidDestination destination = new();

        Assert.Throws<ArgumentNullException>(() => mapper.Map<ValidSource, ValidDestination>(null!, destination, _ => { }));
        Assert.Throws<ArgumentNullException>(() => mapper.Map(source, (ValidDestination)null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => mapper.Map(source, destination, null!));
    }

    [Fact]
    public void Configuration_NullMemberArrays_ThrowArgumentNullException()
    {
        Mapper mapper = new();

        Assert.Throws<ArgumentNullException>(() => mapper.Map<ValidSource, ValidDestination>(
            new ValidSource(),
            map => map.Only(null!)));
        Assert.Throws<ArgumentNullException>(() => mapper.Map<ValidSource, ValidDestination>(
            new ValidSource(),
            map => map.Except(null!)));
    }

    [Fact]
    public void Bind_NullExpressionsAndConverters_ThrowArgumentNullException()
    {
        Mapper mapper = new();

        Assert.Throws<ArgumentNullException>(() => mapper.Map<ValidSource, ValidDestination>(
            new ValidSource(),
            map => map.Bind<string>(null!)));
        Assert.Throws<ArgumentNullException>(() => mapper.Map<ValidSource, ValidDestination>(
            new ValidSource(),
            map => map.Bind(destination => destination.Name).From(null!)));
        Assert.Throws<ArgumentNullException>(() => mapper.Map<ValidSource, ValidDestination>(
            new ValidSource(),
            map => map.Bind(destination => destination.Name).From(
                source => source.Name,
                (Func<string?, string?>)null!)));
    }

    [Fact]
    public void Map_AbstractDestination_ThrowsMappingException()
    {
        MappingException exception = Assert.Throws<MappingException>(() => new Mapper()
            .Map<ValidSource, AbstractDestination>(new ValidSource()));

        Assert.Contains("non-abstract", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Map_DestinationWithoutDefaultConstructor_ThrowsMappingException()
    {
        MappingException exception = Assert.Throws<MappingException>(() => new Mapper()
            .Map<ValidSource, ConstructorDestination>(new ValidSource()));

        Assert.Contains("parameterless constructor", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Map_ThrowingSetter_WrapsOriginalException()
    {
        MappingException exception = Assert.Throws<MappingException>(() => new Mapper()
            .Map<ValidSource, ThrowingDestination>(new ValidSource { Name = "value" }));

        Assert.Equal("The compiled mapping plan could not be executed.", exception.Message);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public void ExistingMap_ThrowingSetter_WrapsOriginalException()
    {
        MappingException exception = Assert.Throws<MappingException>(() => new Mapper().Map(
            new ValidSource { Name = "value" },
            new ThrowingDestination()));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public void ExistingMap_InvalidConversionPreservesMappingException()
    {
        MappingException exception = Assert.Throws<MappingException>(() => new Mapper().Map(
            new InvalidNumberSource { Number = "invalid" },
            new NumberDestination()));

        Assert.Contains("cannot be converted", exception.Message, StringComparison.Ordinal);
        Assert.IsType<FormatException>(exception.InnerException);
    }

    [Fact]
    public void MappingException_ConstructorsPreserveMessageAndInnerException()
    {
        InvalidOperationException inner = new("inner");

        MappingException simple = new("simple");
        MappingException wrapped = new("wrapped", inner);

        Assert.Equal("simple", simple.Message);
        Assert.Equal("wrapped", wrapped.Message);
        Assert.Same(inner, wrapped.InnerException);
    }

    public sealed class ValidSource
    {
        public string? Name { get; set; }
    }

    public sealed class ValidDestination
    {
        public string? Name { get; set; }
    }

    public abstract class AbstractDestination
    {
        public string? Name { get; set; }
    }

    public sealed class ConstructorDestination
    {
        public ConstructorDestination(string value)
        {
            Name = value;
        }

        public string Name { get; set; }
    }

    public sealed class ThrowingDestination
    {
        public string? Name
        {
            get => null;
            set => throw new InvalidOperationException(value);
        }
    }

    public sealed class InvalidNumberSource
    {
        public string Number { get; set; } = string.Empty;
    }

    public sealed class NumberDestination
    {
        public int Number { get; set; }
    }
}
