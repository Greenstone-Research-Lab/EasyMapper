namespace EasyMapper;

/// <summary>
/// Represents a failure to construct, compile, or execute an EasyMapper mapping plan.
/// </summary>
public class MappingException : InvalidOperationException
{
    /// <summary>
    /// Initializes a mapping exception with a descriptive message.
    /// </summary>
    /// <param name="message">The explanation of the mapping failure.</param>
    public MappingException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a mapping exception with a descriptive message and originating exception.
    /// </summary>
    /// <param name="message">The explanation of the mapping failure.</param>
    /// <param name="innerException">The exception that caused the mapping failure.</param>
    public MappingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
