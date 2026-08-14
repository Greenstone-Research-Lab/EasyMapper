using System.Collections.Concurrent;

namespace EasyMapper.Internal;

internal sealed class MappingPlanProvider
{
    private readonly ConcurrentDictionary<(Type Source, Type Destination), MappingPlan> _cache = new();
    private readonly MappingPlanCompiler _compiler;

    public MappingPlanProvider(MapperOptions options)
    {
        _compiler = new MappingPlanCompiler(options);
    }

    public MappingPlan GetOrCreate(Type source, Type destination) =>
        _cache.GetOrAdd((source, destination), key => _compiler.Compile(key.Source, key.Destination, null));

    public MappingPlan Create(Type source, Type destination, MappingDefinition definition) =>
        _compiler.Compile(source, destination, definition);
}
