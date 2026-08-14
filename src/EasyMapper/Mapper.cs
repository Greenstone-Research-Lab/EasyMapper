using EasyMapper.Internal;

namespace EasyMapper;

/// <summary>
/// Maps objects by compiling convention and lambda rules into reusable execution plans.
/// </summary>
/// <remarks>
/// A <see cref="Mapper"/> is thread-safe. Construct one instance per convention set and reuse it
/// throughout the application to obtain the full benefit of mapping-plan caching.
/// </remarks>
public sealed class Mapper : IMapper
{
    private readonly MappingPlanProvider _plans;

    /// <summary>
    /// Initializes a mapper with the default, case-sensitive conventions.
    /// </summary>
    public Mapper()
        : this(new MapperOptions())
    {
    }

    /// <summary>
    /// Initializes a mapper with a snapshot of the supplied conventions.
    /// </summary>
    /// <param name="options">The conventions used for all plans produced by this mapper.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public Mapper(MapperOptions options)
    {
        Guard.NotNull(options, nameof(options));
        _plans = new MappingPlanProvider(options.Snapshot());
    }

    /// <inheritdoc />
    public TDestination Map<TDestination>(object source)
        where TDestination : class
    {
        Guard.NotNull(source, nameof(source));
        return (TDestination)_plans.GetOrCreate(source.GetType(), typeof(TDestination)).CreateAndMap(source);
    }

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(TSource source)
        where TSource : notnull
        where TDestination : class
    {
        Guard.NotNull(source, nameof(source));
        return (TDestination)_plans.GetOrCreate(typeof(TSource), typeof(TDestination)).CreateAndMap(source);
    }

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(
        TSource source,
        Action<IMappingExpression<TSource, TDestination>> configure)
        where TSource : notnull
        where TDestination : class
    {
        Guard.NotNull(source, nameof(source));
        Guard.NotNull(configure, nameof(configure));

        MappingExpression<TSource, TDestination> expression = new();
        configure(expression);
        return (TDestination)_plans.Create(typeof(TSource), typeof(TDestination), expression.Definition).CreateAndMap(source);
    }

    /// <inheritdoc />
    public void Map<TSource, TDestination>(TSource source, TDestination destination)
        where TSource : notnull
        where TDestination : class
    {
        Guard.NotNull(source, nameof(source));
        Guard.NotNull(destination, nameof(destination));
        _plans.GetOrCreate(typeof(TSource), typeof(TDestination)).Map(source, destination);
    }

    /// <inheritdoc />
    public void Map<TSource, TDestination>(
        TSource source,
        TDestination destination,
        Action<IMappingExpression<TSource, TDestination>> configure)
        where TSource : notnull
        where TDestination : class
    {
        Guard.NotNull(source, nameof(source));
        Guard.NotNull(destination, nameof(destination));
        Guard.NotNull(configure, nameof(configure));

        MappingExpression<TSource, TDestination> expression = new();
        configure(expression);
        _plans.Create(typeof(TSource), typeof(TDestination), expression.Definition).Map(source, destination);
    }
}
