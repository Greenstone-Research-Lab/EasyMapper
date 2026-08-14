namespace EasyMapper;

/// <summary>
/// Provides a zero-configuration facade for convention-based object mapping.
/// </summary>
/// <remarks>
/// Mapping plans are created on first use and cached for subsequent calls. Create a dedicated
/// <see cref="Mapper"/> when application-specific conventions are required.
/// </remarks>
public static class EasyMapper
{
    private static readonly Mapper DefaultMapper = new();

    /// <summary>
    /// Creates and populates a destination object from the source object's runtime type.
    /// </summary>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>A newly created destination instance.</returns>
    /// <example>
    /// <code>
    /// UserDto dto = EasyMapper.Map&lt;UserDto&gt;(user);
    /// </code>
    /// </example>
    public static TDestination Map<TDestination>(object source)
        where TDestination : class => DefaultMapper.Map<TDestination>(source);

    /// <summary>
    /// Creates and populates a destination object using the specified compile-time type pair.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The source instance.</param>
    /// <returns>A newly created destination instance.</returns>
    public static TDestination Map<TSource, TDestination>(TSource source)
        where TSource : notnull
        where TDestination : class => DefaultMapper.Map<TSource, TDestination>(source);

    /// <summary>
    /// Creates a destination object using conventions and strongly typed exceptions declared for this call.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The source instance.</param>
    /// <param name="configure">The optional mapping exceptions.</param>
    /// <returns>A newly created destination instance.</returns>
    public static TDestination Map<TSource, TDestination>(
        TSource source,
        Action<IMappingExpression<TSource, TDestination>> configure)
        where TSource : notnull
        where TDestination : class => DefaultMapper.Map(source, configure);

    /// <summary>
    /// Copies convention-matched values into an existing destination object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The source instance.</param>
    /// <param name="destination">The destination instance to update.</param>
    public static void Map<TSource, TDestination>(TSource source, TDestination destination)
        where TSource : notnull
        where TDestination : class => DefaultMapper.Map(source, destination);

    /// <summary>
    /// Copies values into an existing destination using conventions and call-specific lambda rules.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The source instance.</param>
    /// <param name="destination">The destination instance to update.</param>
    /// <param name="configure">The optional mapping exceptions.</param>
    public static void Map<TSource, TDestination>(
        TSource source,
        TDestination destination,
        Action<IMappingExpression<TSource, TDestination>> configure)
        where TSource : notnull
        where TDestination : class => DefaultMapper.Map(source, destination, configure);
}
