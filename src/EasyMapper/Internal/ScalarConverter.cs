using System.Globalization;

namespace EasyMapper.Internal;

internal static class ScalarConverter
{
    public static bool CanConvert(Type sourceType, Type destinationType)
    {
        Type source = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
        Type destination = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

        return destination.IsAssignableFrom(source) ||
               destination.IsEnum ||
               source.IsEnum ||
               destination == typeof(Guid) ||
               destination == typeof(TimeSpan) ||
               typeof(IConvertible).IsAssignableFrom(source) && typeof(IConvertible).IsAssignableFrom(destination);
    }

    public static object? ConvertValue(object? value, Type destinationType)
    {
        Type target = Nullable.GetUnderlyingType(destinationType) ?? destinationType;
        if (value is null)
        {
            if (destinationType.IsValueType && Nullable.GetUnderlyingType(destinationType) is null)
            {
                throw new MappingException($"A null value cannot be assigned to '{destinationType}'.");
            }

            return null;
        }

        Type source = value.GetType();
        if (target.IsAssignableFrom(source))
        {
            return value;
        }

        try
        {
            if (target.IsEnum)
            {
                return value is string text
                    ? Enum.Parse(target, text, ignoreCase: true)
                    : Enum.ToObject(target, value);
            }

            if (source.IsEnum)
            {
                value = System.Convert.ChangeType(value, Enum.GetUnderlyingType(source), CultureInfo.InvariantCulture);
            }

            if (target == typeof(Guid) && value is string guid)
            {
                return Guid.Parse(guid);
            }

            if (target == typeof(TimeSpan) && value is string duration)
            {
                return TimeSpan.Parse(duration, CultureInfo.InvariantCulture);
            }

            return System.Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException or ArgumentException)
        {
            throw new MappingException(
                $"Value of type '{source}' cannot be converted to '{destinationType}'.",
                exception);
        }
    }
}
