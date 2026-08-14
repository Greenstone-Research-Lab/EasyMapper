namespace EasyMapper.Internal;

internal sealed class MappingPlan
{
    private readonly Func<object> _factory;
    private readonly Action<object, object> _map;

    public MappingPlan(Func<object> factory, Action<object, object> map)
    {
        _factory = factory;
        _map = map;
    }

    public object CreateAndMap(object source)
    {
        object destination;
        try
        {
            destination = _factory();
            _map(source, destination);
        }
        catch (MappingException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new MappingException("The compiled mapping plan could not be executed.", exception);
        }

        return destination;
    }

    public void Map(object source, object destination)
    {
        try
        {
            _map(source, destination);
        }
        catch (MappingException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new MappingException("The compiled mapping plan could not be executed.", exception);
        }
    }
}
