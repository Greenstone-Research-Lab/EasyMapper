using System.Linq.Expressions;

namespace EasyMapper;

/// <summary>
/// Describes strongly typed exceptions to EasyMapper's convention-based member matching.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMappingExpression<TSource, TDestination>
{
    /// <summary>
    /// Prevents one or more destination properties from receiving mapped values.
    /// </summary>
    /// <param name="members">Direct destination-property expressions.</param>
    /// <returns>The current expression for continued fluent configuration.</returns>
    IMappingExpression<TSource, TDestination> Except(
        params Expression<Func<TDestination, object?>>[] members);

    /// <summary>
    /// Restricts the plan to the specified destination properties.
    /// </summary>
    /// <param name="members">Direct destination-property expressions.</param>
    /// <returns>The current expression for continued fluent configuration.</returns>
    IMappingExpression<TSource, TDestination> Only(
        params Expression<Func<TDestination, object?>>[] members);

    /// <summary>
    /// Begins an explicit mapping rule for a destination property.
    /// </summary>
    /// <typeparam name="TMember">The destination property's value type.</typeparam>
    /// <param name="destination">A direct destination-property expression.</param>
    /// <returns>A member expression that accepts the corresponding source expression.</returns>
    IMemberMappingExpression<TSource, TDestination, TMember> Bind<TMember>(
        Expression<Func<TDestination, TMember>> destination);
}

/// <summary>
/// Completes an explicit destination-member rule with a source expression or conversion.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
/// <typeparam name="TDestinationMember">The destination member type.</typeparam>
public interface IMemberMappingExpression<TSource, TDestination, TDestinationMember>
{
    /// <summary>
    /// Maps the destination property from a source expression with the same resulting type.
    /// </summary>
    /// <param name="source">An expression that calculates the destination value.</param>
    /// <returns>The parent mapping expression.</returns>
    IMappingExpression<TSource, TDestination> From(
        Expression<Func<TSource, TDestinationMember>> source);

    /// <summary>
    /// Maps the destination property from a source expression and an explicit converter.
    /// </summary>
    /// <typeparam name="TSourceMember">The source expression's value type.</typeparam>
    /// <param name="source">An expression that selects or calculates the source value.</param>
    /// <param name="converter">A deterministic conversion delegate.</param>
    /// <returns>The parent mapping expression.</returns>
    /// <example>
    /// <code>
    /// map.Bind(d => d.Id)
    ///    .From(s => s.ExternalId, value => Guid.Parse(value));
    /// </code>
    /// </example>
    IMappingExpression<TSource, TDestination> From<TSourceMember>(
        Expression<Func<TSource, TSourceMember>> source,
        Func<TSourceMember, TDestinationMember> converter);
}
