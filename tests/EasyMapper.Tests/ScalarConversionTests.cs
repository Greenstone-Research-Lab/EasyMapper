namespace EasyMapper.Tests;

public sealed class ScalarConversionTests
{
    [Theory]
    [InlineData("42", 42)]
    [InlineData("-7", -7)]
    public void Map_ConvertsStringsToIntegers(string value, int expected)
    {
        NumericDestination result = new Mapper().Map<TextSource, NumericDestination>(new TextSource { Number = value });

        Assert.Equal(expected, result.Number);
    }

    [Fact]
    public void Map_ConvertsEnumNamesIgnoringCase()
    {
        EnumDestination result = new Mapper().Map<EnumTextSource, EnumDestination>(
            new EnumTextSource { Status = "active" });

        Assert.Equal(Status.Active, result.Status);
    }

    [Fact]
    public void Map_ConvertsEnumToNumericValue()
    {
        NumericStatusDestination result = new Mapper().Map<EnumSource, NumericStatusDestination>(
            new EnumSource { Status = Status.Active });

        Assert.Equal(1, result.Status);
    }

    [Fact]
    public void Map_ConvertsNumericValueToEnum()
    {
        EnumDestination result = new Mapper().Map<NumericStatusSource, EnumDestination>(
            new NumericStatusSource { Status = 1 });

        Assert.Equal(Status.Active, result.Status);
    }

    [Fact]
    public void Map_ConvertsGuidAndTimeSpanStrings()
    {
        string id = "5f1d7d67-52db-47d0-af92-016ff5eae987";
        SpecialDestination result = new Mapper().Map<SpecialSource, SpecialDestination>(new SpecialSource
        {
            Id = id,
            Duration = "01:02:03",
        });

        Assert.Equal(Guid.Parse(id), result.Id);
        Assert.Equal(new TimeSpan(1, 2, 3), result.Duration);
    }

    [Fact]
    public void Map_ConvertsNullableNumericValues()
    {
        NullableLongDestination result = new Mapper().Map<NullableIntSource, NullableLongDestination>(
            new NullableIntSource { Value = 12 });

        Assert.Equal(12L, result.Value);
    }

    [Fact]
    public void Map_PreservesNullNullableValues()
    {
        NullableLongDestination result = new Mapper().Map<NullableIntSource, NullableLongDestination>(
            new NullableIntSource { Value = null });

        Assert.Null(result.Value);
    }

    [Fact]
    public void Map_ConvertsNullableValueToNonNullableValue()
    {
        NonNullableIntDestination result = new Mapper().Map<NullableIntSource, NonNullableIntDestination>(
            new NullableIntSource { Value = 12 });

        Assert.Equal(12, result.Value);
    }

    [Fact]
    public void Map_NullToNonNullableValue_ThrowsMappingException()
    {
        MappingException exception = Assert.Throws<MappingException>(() => new Mapper()
            .Map<NullableTextSource, NumericValueDestination>(new NullableTextSource { Value = null }));

        Assert.Contains("null value", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Map_InvalidScalarValue_ThrowsMappingExceptionWithInnerException()
    {
        MappingException exception = Assert.Throws<MappingException>(() => new Mapper()
            .Map<TextSource, NumericDestination>(new TextSource { Number = "not-a-number" }));

        Assert.Contains("cannot be converted", exception.Message, StringComparison.Ordinal);
        Assert.IsType<FormatException>(exception.InnerException);
    }

    [Fact]
    public void Map_WhenScalarConversionsDisabled_SkipsDifferentTypes()
    {
        Mapper mapper = new(new MapperOptions { AllowScalarConversions = false });

        NumericDestination result = mapper.Map<TextSource, NumericDestination>(new TextSource { Number = "42" });

        Assert.Equal(0, result.Number);
    }

    [Fact]
    public void Map_IncompatibleComplexTypes_AreSkipped()
    {
        ComplexDestination result = new Mapper().Map<ComplexSource, ComplexDestination>(new ComplexSource
        {
            Value = new Uri("https://example.com"),
        });

        Assert.Null(result.Value);
    }

    public sealed class TextSource
    {
        public string Number { get; set; } = string.Empty;
    }

    public sealed class NumericDestination
    {
        public int Number { get; set; }
    }

    public enum Status
    {
        Unknown,
        Active,
    }

    public sealed class EnumTextSource
    {
        public string Status { get; set; } = string.Empty;
    }

    public sealed class EnumDestination
    {
        public Status Status { get; set; }
    }

    public sealed class EnumSource
    {
        public Status Status { get; set; }
    }

    public sealed class NumericStatusDestination
    {
        public int Status { get; set; }
    }

    public sealed class NumericStatusSource
    {
        public int Status { get; set; }
    }

    public sealed class SpecialSource
    {
        public string Id { get; set; } = string.Empty;

        public string Duration { get; set; } = string.Empty;
    }

    public sealed class SpecialDestination
    {
        public Guid Id { get; set; }

        public TimeSpan Duration { get; set; }
    }

    public sealed class NullableIntSource
    {
        public int? Value { get; set; }
    }

    public sealed class NullableLongDestination
    {
        public long? Value { get; set; }
    }

    public sealed class NonNullableIntDestination
    {
        public int Value { get; set; }
    }

    public sealed class NullableTextSource
    {
        public string? Value { get; set; }
    }

    public sealed class NumericValueDestination
    {
        public int Value { get; set; }
    }

    public sealed class ComplexSource
    {
        public Uri? Value { get; set; }
    }

    public sealed class ComplexDestination
    {
        public Version? Value { get; set; }
    }
}
