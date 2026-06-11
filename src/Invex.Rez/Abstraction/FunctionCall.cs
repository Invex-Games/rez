namespace Invex.Rez.Abstraction;

/// <summary>
///     Contains the arguments for a Rez function call.
///     <br /><br />
///     When an <see cref="IResolver" /> encounters a function in a Rez template (e.g., <c>{name(args)}</c>),
///     it constructs a <see cref="FunctionCall" /> from the text between the parentheses and passes it to the
///     function delegate obtained from an <see cref="IResolverSource" />.
/// </summary>
/// <param name="Args">
///     The raw argument text of the function call, as a single string.<br />
///     For example, for the template <c>{repeat(abc,3)}</c>, this would be <c>"abc,3"</c>.<br />
///     The function itself is responsible for parsing this text; no splitting or trimming is performed.
/// </param>
[PublicAPI]
public readonly record struct FunctionCall(string? Args);
