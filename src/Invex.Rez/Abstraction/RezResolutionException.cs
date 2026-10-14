namespace Invex.Rez.Abstraction;

/// <summary>
///     The exception thrown when a Rez template cannot be resolved, such as when resolution exceeds the
///     maximum recursion depth because of a cycle whose output keeps changing (e.g., <c>a = {b}</c> and
///     <c>b = {a}</c>).
/// </summary>
/// <remarks>
///     Derives directly from <see cref="Exception" />, so existing <c>catch (Exception)</c> handlers continue to work.
/// </remarks>
[PublicAPI]
public class RezResolutionException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RezResolutionException" /> class.
    /// </summary>
    public RezResolutionException() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RezResolutionException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RezResolutionException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RezResolutionException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RezResolutionException(string message, Exception innerException) : base(message, innerException) { }
}
