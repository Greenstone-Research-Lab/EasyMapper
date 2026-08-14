using System.Linq.Expressions;
using System.Reflection;

namespace EasyMapper.Internal;

internal sealed class MappingPlanCompiler
{
    private static readonly MethodInfo ConvertValueMethod = typeof(ScalarConverter)
        .GetMethod(nameof(ScalarConverter.ConvertValue), BindingFlags.Public | BindingFlags.Static)!;

    private readonly MapperOptions _options;

    public MappingPlanCompiler(MapperOptions options)
    {
        _options = options;
    }

    public MappingPlan Compile(Type sourceType, Type destinationType, MappingDefinition? definition)
    {
        ValidateTypes(sourceType, destinationType);
        Func<object> factory = CompileFactory(destinationType);
        Action<object, object> mapper = CompileMapper(sourceType, destinationType, definition);
        return new MappingPlan(factory, mapper);
    }

    private static void ValidateTypes(Type sourceType, Type destinationType)
    {
        if (!destinationType.IsClass || destinationType.IsAbstract)
        {
            throw new MappingException($"Destination type '{destinationType}' must be a non-abstract class.");
        }

        if (destinationType.GetConstructor(Type.EmptyTypes) is null)
        {
            throw new MappingException($"Destination type '{destinationType}' must declare a public parameterless constructor.");
        }
    }

    private static Func<object> CompileFactory(Type destinationType)
    {
        NewExpression create = Expression.New(destinationType);
        return Expression.Lambda<Func<object>>(Expression.Convert(create, typeof(object))).Compile();
    }

    private Action<object, object> CompileMapper(
        Type sourceType,
        Type destinationType,
        MappingDefinition? definition)
    {
        ParameterExpression sourceObject = Expression.Parameter(typeof(object), "source");
        ParameterExpression destinationObject = Expression.Parameter(typeof(object), "destination");
        UnaryExpression source = Expression.Convert(sourceObject, sourceType);
        UnaryExpression destination = Expression.Convert(destinationObject, destinationType);

        StringComparer comparer = _options.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        Dictionary<string, PropertyInfo> sourceProperties = sourceType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(IsReadable)
            .GroupBy(property => property.Name, comparer)
            .ToDictionary(group => group.Key, group => group.First(), comparer);

        List<Expression> assignments = new();
        foreach (PropertyInfo destinationProperty in destinationType
                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .Where(IsWritable))
        {
            if (definition is not null && !definition.ShouldMap(destinationProperty))
            {
                continue;
            }

            Expression? value = CreateValueExpression(
                source,
                sourceProperties,
                destinationProperty,
                definition);
            if (value is null)
            {
                continue;
            }

            MemberExpression target = Expression.Property(destination, destinationProperty);
            BinaryExpression assignment = Expression.Assign(target, value);
            assignments.Add(ShouldGuardNull(value)
                ? Expression.IfThen(Expression.NotEqual(value, Expression.Constant(null, value.Type)), assignment)
                : assignment);
        }

        BlockExpression body = Expression.Block(assignments);
        return Expression.Lambda<Action<object, object>>(body, sourceObject, destinationObject).Compile();
    }

    private Expression? CreateValueExpression(
        Expression source,
        Dictionary<string, PropertyInfo> sourceProperties,
        PropertyInfo destination,
        MappingDefinition? definition)
    {
        if (definition is not null && definition.Bindings.TryGetValue(destination, out LambdaExpression? binding))
        {
            return new ParameterReplaceVisitor(binding.Parameters[0], source).Visit(binding.Body);
        }

        if (!sourceProperties.TryGetValue(destination.Name, out PropertyInfo? sourceProperty))
        {
            return null;
        }

        MemberExpression value = Expression.Property(source, sourceProperty);
        if (destination.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
        {
            return value;
        }

        if (!_options.AllowScalarConversions || !ScalarConverter.CanConvert(sourceProperty.PropertyType, destination.PropertyType))
        {
            return null;
        }

        MethodCallExpression converted = Expression.Call(
            ConvertValueMethod,
            Expression.Convert(value, typeof(object)),
            Expression.Constant(destination.PropertyType));
        return Expression.Convert(converted, destination.PropertyType);
    }

    private bool ShouldGuardNull(Expression value) =>
        _options.IgnoreNullValues && (!value.Type.IsValueType || Nullable.GetUnderlyingType(value.Type) is not null);

    private static bool IsReadable(PropertyInfo property) =>
        property.GetMethod is { IsPublic: true } && property.GetIndexParameters().Length == 0;

    private static bool IsWritable(PropertyInfo property) =>
        property.SetMethod is { IsPublic: true } && property.GetIndexParameters().Length == 0;
}
