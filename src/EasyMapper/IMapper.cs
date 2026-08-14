namespace EasyMapper;

/// <summary>
/// Defines convention-based and explicitly configured object-mapping operations.
/// </summary>
/// <remarks>
/// Implementations are expected to be thread-safe. A mapper may therefore be registered as a
/// singleton in a dependency-injection container or shared directly by an application.
/// </remarks>
public interface IMapper
{
    /// <summary>
    /// Creates a new <typeparamref name="TDestination"/> instance and copies compatible members
    /// from the runtime type of <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The non-null source instance.</param>
    /// <returns>A newly created and populated destination instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="MappingException">The destination cannot be created or a mapped value cannot be assigned.</exception>
    TDestination Map<TDestination>(object source)
        where TDestination : class;

    /// <summary>
    /// Creates a destination instance by applying the cached convention plan for the specified type pair.
    /// </summary>
    /// <typeparam name="TSource">The compile-time source type.</typeparam>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The non-null source instance.</param>
    /// <returns>A newly created and populated destination instance.</returns>
    TDestination Map<TSource, TDestination>(TSource source)
        where TSource : notnull
        where TDestination : class;

    /// <summary>
    /// Creates a destination instance using convention mapping supplemented by strongly typed, call-specific rules.
    /// </summary>
    /// <typeparam name="TSource">The compile-time source type.</typeparam>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The non-null source instance.</param>
    /// <param name="configure">A callback that describes only the exceptions to convention mapping.</param>
    /// <returns>A newly created and populated destination instance.</returns>
    /// <example>
    /// <code>
    /// var dto = mapper.Map&lt;User, UserDto&gt;(user, map => map
    ///     .Bind(d => d.DisplayName).From(s => s.FirstName + " " + s.LastName)
    ///     .Except(d => d.PasswordHash));
    /// </code>
    /// </example>
    TDestination Map<TSource, TDestination>(
        TSource source,
        Action<IMappingExpression<TSource, TDestination>> configure)
        where TSource : notnull
        where TDestination : class;

    /// <summary>
    /// Copies compatible members from a source instance into an existing destination instance.
    /// </summary>
    /// <typeparam name="TSource">The compile-time source type.</typeparam>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The non-null source instance.</param>
    /// <param name="destination">The non-null destination instance to update.</param>
    void Map<TSource, TDestination>(TSource source, TDestination destination)
        where TSource : notnull
        where TDestination : class;

    /// <summary>
    /// Copies members into an existing destination using convention mapping and call-specific lambda rules.
    /// </summary>
    /// <typeparam name="TSource">The compile-time source type.</typeparam>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The non-null source instance.</param>
    /// <param name="destination">The non-null destination instance to update.</param>
    /// <param name="configure">A callback that describes only the exceptions to convention mapping.</param>
    void Map<TSource, TDestination>(
        TSource source,
        TDestination destination,
        Action<IMappingExpression<TSource, TDestination>> configure)
        where TSource : notnull
        where TDestination : class;
}
