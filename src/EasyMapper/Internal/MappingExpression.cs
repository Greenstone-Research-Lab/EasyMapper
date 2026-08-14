using System.Linq.Expressions;
using System.Reflection;

namespace EasyMapper.Internal;

internal sealed class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
{
    public MappingDefinition Definition { get; } = new();

    public IMappingExpression<TSource, TDestination> Except(
        params Expression<Func<TDestination, object?>>[] members)
    {
        Guard.NotNull(members, nameof(members));
        foreach (Expression<Func<TDestination, object?>> member in members)
        {
            Definition.Exclude(MemberExpressionReader.ReadProperty(member, nameof(members)));
        }

        return this;
    }

    public IMappingExpression<TSource, TDestination> Only(
        params Expression<Func<TDestination, object?>>[] members)
    {
        Guard.NotNull(members, nameof(members));
        foreach (Expression<Func<TDestination, object?>> member in members)
        {
            Definition.Include(MemberExpressionReader.ReadProperty(member, nameof(members)));
        }

        return this;
    }

    public IMemberMappingExpression<TSource, TDestination, TMember> Bind<TMember>(
        Expression<Func<TDestination, TMember>> destination)
    {
        Guard.NotNull(destination, nameof(destination));
        PropertyInfo property = MemberExpressionReader.ReadProperty(destination, nameof(destination));
        if (property.SetMethod is null || !property.SetMethod.IsPublic)
        {
            throw new ArgumentException(
                $"Destination property '{property.Name}' must have a public setter.",
                nameof(destination));
        }

        return new MemberMappingExpression<TSource, TDestination, TMember>(this, property);
    }
}

internal sealed class MemberMappingExpression<TSource, TDestination, TDestinationMember>
    : IMemberMappingExpression<TSource, TDestination, TDestinationMember>
{
    private readonly MappingExpression<TSource, TDestination> _parent;
    private readonly PropertyInfo _destination;

    public MemberMappingExpression(
        MappingExpression<TSource, TDestination> parent,
        PropertyInfo destination)
    {
        _parent = parent;
        _destination = destination;
    }

    public IMappingExpression<TSource, TDestination> From(
        Expression<Func<TSource, TDestinationMember>> source)
    {
        Guard.NotNull(source, nameof(source));
        _parent.Definition.Bind(_destination, source);
        return _parent;
    }

    public IMappingExpression<TSource, TDestination> From<TSourceMember>(
        Expression<Func<TSource, TSourceMember>> source,
        Func<TSourceMember, TDestinationMember> converter)
    {
        Guard.NotNull(source, nameof(source));
        Guard.NotNull(converter, nameof(converter));

        InvocationExpression converted = Expression.Invoke(Expression.Constant(converter), source.Body);
        Expression<Func<TSource, TDestinationMember>> binding = Expression.Lambda<Func<TSource, TDestinationMember>>(
            converted,
            source.Parameters);
        _parent.Definition.Bind(_destination, binding);
        return _parent;
    }
}
