using System.Linq.Expressions;
using System.Reflection;

namespace EasyMapper.Internal;

internal sealed class MappingDefinition
{
    private readonly Dictionary<PropertyInfo, LambdaExpression> _bindings = new();
    private readonly HashSet<PropertyInfo> _excluded = new();
    private readonly HashSet<PropertyInfo> _included = new();

    public IReadOnlyDictionary<PropertyInfo, LambdaExpression> Bindings => _bindings;

    public bool HasIncludes => _included.Count != 0;

    public void Bind(PropertyInfo destination, LambdaExpression source) => _bindings[destination] = source;

    public void Exclude(PropertyInfo property) => _excluded.Add(property);

    public void Include(PropertyInfo property) => _included.Add(property);

    public bool ShouldMap(PropertyInfo property) =>
        !_excluded.Contains(property) && (!HasIncludes || _included.Contains(property));
}
