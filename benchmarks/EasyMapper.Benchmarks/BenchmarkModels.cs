namespace EasyMapper.Benchmarks;

/// <summary>
/// Represents the source object shared by the convention-based benchmark scenarios.
/// </summary>
public sealed class MappingSource
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CorrelationId { get; set; }

    public bool IsActive { get; set; }

    public long Sequence { get; set; }
}

/// <summary>
/// Represents the destination object shared by the convention-based benchmark scenarios.
/// </summary>
public sealed class MappingDestination
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CorrelationId { get; set; }

    public bool IsActive { get; set; }

    public long Sequence { get; set; }
}

/// <summary>
/// Represents a source object that requires an explicit calculated-member rule.
/// </summary>
public sealed class PersonSource
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a destination object with a calculated display name.
/// </summary>
public sealed class PersonDestination
{
    public int Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}

internal static class BenchmarkData
{
    public static MappingSource CreateSource(int id = 42) => new()
    {
        Id = id,
        Name = "Ada Lovelace",
        Email = "ada@example.com",
        Balance = 1843.12m,
        CreatedAt = new DateTimeOffset(1843, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CorrelationId = new Guid("ac3bf05b-89ae-49e1-b74d-8ec2969e8189"),
        IsActive = true,
        Sequence = 9_223_372L + id
    };

    public static MappingDestination MapManually(MappingSource source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email,
        Balance = source.Balance,
        CreatedAt = source.CreatedAt,
        CorrelationId = source.CorrelationId,
        IsActive = source.IsActive,
        Sequence = source.Sequence
    };

    public static void MapManually(MappingSource source, MappingDestination destination)
    {
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Email = source.Email;
        destination.Balance = source.Balance;
        destination.CreatedAt = source.CreatedAt;
        destination.CorrelationId = source.CorrelationId;
        destination.IsActive = source.IsActive;
        destination.Sequence = source.Sequence;
    }

    public static void ValidateEquivalent(MappingSource source, MappingDestination destination)
    {
        if (source.Id != destination.Id ||
            !string.Equals(source.Name, destination.Name, StringComparison.Ordinal) ||
            !string.Equals(source.Email, destination.Email, StringComparison.Ordinal) ||
            source.Balance != destination.Balance ||
            source.CreatedAt != destination.CreatedAt ||
            source.CorrelationId != destination.CorrelationId ||
            source.IsActive != destination.IsActive ||
            source.Sequence != destination.Sequence)
        {
            throw new InvalidOperationException("A benchmark mapper produced a non-equivalent destination.");
        }
    }
}
