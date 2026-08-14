namespace EasyMapper;

/// <summary>
/// Supplies discoverable extension-method syntax for EasyMapper's default mapper.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Creates a destination instance and maps compatible values from the source object's runtime type.
    /// </summary>
    /// <typeparam name="TDestination">The destination reference type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>A newly created destination instance.</returns>
    /// <example>
    /// <code>
    /// UserDto dto = user.MapTo&lt;UserDto&gt;();
    /// </code>
    /// </example>
    public static TDestination MapTo<TDestination>(this object source)
        where TDestination : class => EasyMapper.Map<TDestination>(source);
}
