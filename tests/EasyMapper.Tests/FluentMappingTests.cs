namespace EasyMapper.Tests;

public sealed class FluentMappingTests
{
    [Fact]
    public void Except_DoesNotMapSelectedProperty()
    {
        Account source = new() { Id = 42, Name = "Ada", Secret = "private" };

        AccountDto result = global::EasyMapper.EasyMapper.Map<Account, AccountDto>(
            source,
            map => map.Except(destination => destination.Secret));

        Assert.Equal(42, result.Id);
        Assert.Equal("Ada", result.Name);
        Assert.Null(result.Secret);
    }

    [Fact]
    public void Only_MapsSelectedProperties()
    {
        Account source = new() { Id = 42, Name = "Ada", Secret = "private" };

        AccountDto result = new Mapper().Map<Account, AccountDto>(
            source,
            map => map.Only(destination => destination.Name, destination => destination.Id));

        Assert.Equal(42, result.Id);
        Assert.Equal("Ada", result.Name);
        Assert.Null(result.Secret);
    }

    [Fact]
    public void Bind_FromExpression_MapsCalculatedValue()
    {
        Person source = new() { FirstName = "Ada", LastName = "Lovelace" };

        PersonDto result = global::EasyMapper.EasyMapper.Map<Person, PersonDto>(source, map => map
            .Bind(destination => destination.DisplayName)
            .From(value => value.FirstName + " " + value.LastName));

        Assert.Equal("Ada Lovelace", result.DisplayName);
    }

    [Fact]
    public void Bind_WithConverter_MapsDifferentTypes()
    {
        ExternalRecord source = new() { ExternalId = "5f1d7d67-52db-47d0-af92-016ff5eae987" };

        InternalRecord result = new Mapper().Map<ExternalRecord, InternalRecord>(source, map => map
            .Bind(destination => destination.Id)
            .From(value => value.ExternalId, Guid.Parse));

        Assert.Equal(Guid.Parse(source.ExternalId), result.Id);
    }

    [Fact]
    public void ExistingDestination_WithRules_AppliesRulesInPlace()
    {
        Account source = new() { Id = 9, Name = "new", Secret = "source" };
        AccountDto destination = new() { Secret = "existing" };

        global::EasyMapper.EasyMapper.Map(
            source,
            destination,
            map => map.Except(value => value.Secret));

        Assert.Equal(9, destination.Id);
        Assert.Equal("new", destination.Name);
        Assert.Equal("existing", destination.Secret);
    }

    [Fact]
    public void Bind_NonPropertyExpression_ThrowsDescriptiveArgumentException()
    {
        Mapper mapper = new();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => mapper.Map<Person, PersonDto>(
            new Person(),
            map => map.Bind(destination => destination.DisplayName.Length).From(_ => 0)));

        Assert.Contains("direct property", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Except_NestedPropertyExpression_ThrowsDescriptiveArgumentException()
    {
        Mapper mapper = new();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => mapper.Map<Person, PersonDto>(
            new Person(),
            map => map.Except(destination => destination.DisplayName.Length)));

        Assert.Contains("direct property", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Bind_ReadOnlyDestination_ThrowsDescriptiveArgumentException()
    {
        Mapper mapper = new();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => mapper.Map<Person, ReadOnlyPersonDto>(
            new Person(),
            map => map.Bind(destination => destination.DisplayName).From(source => source.FirstName)));

        Assert.Contains("public setter", exception.Message, StringComparison.Ordinal);
    }

    public sealed class Account
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Secret { get; set; }
    }

    public sealed class AccountDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Secret { get; set; }
    }

    public sealed class Person
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }

    public sealed class PersonDto
    {
        public string DisplayName { get; set; } = string.Empty;
    }

    public sealed class ReadOnlyPersonDto
    {
        public string DisplayName => string.Empty;
    }

    public sealed class ExternalRecord
    {
        public string ExternalId { get; set; } = string.Empty;
    }

    public sealed class InternalRecord
    {
        public Guid Id { get; set; }
    }
}
