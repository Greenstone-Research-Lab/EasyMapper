namespace EasyMapper;

/// <summary>
/// Controls the conventions used when EasyMapper constructs automatic mapping plans.
/// </summary>
public sealed class MapperOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether source and destination property names must use identical casing.
    /// </summary>
    /// <value><see langword="true"/> by default.</value>
    public bool CaseSensitive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a null source property leaves the existing destination value unchanged.
    /// </summary>
    /// <value><see langword="false"/> by default, so null values are assigned when legal.</value>
    public bool IgnoreNullValues { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether common scalar conversions are attempted for differently typed members.
    /// </summary>
    /// <value><see langword="true"/> by default.</value>
    public bool AllowScalarConversions { get; set; } = true;

    internal MapperOptions Snapshot() => new()
    {
        CaseSensitive = CaseSensitive,
        IgnoreNullValues = IgnoreNullValues,
        AllowScalarConversions = AllowScalarConversions,
    };
}
