namespace Invex.Rez.Abstraction;

/// <summary>
///     Provides functionality for resolving variables and functions in a Rez template into an output string.
/// </summary>
[PublicAPI]
public interface IResolver
{
    /// <summary>
    ///     Adds a new source to the resolver.<br />
    ///     If the source has already been added, this method does nothing.
    /// </summary>
    /// <param name="source">The source to add.</param>
    /// <returns>The resolver instance, allowing calls to be chained.</returns>
    /// <remarks>
    ///     The order in which sources are added is important - when resolving a variable or function,
    ///     sources are queried in the order they were added, and the first non-null result wins.
    /// </remarks>
    IResolver AddSource(IResolverSource source);

    /// <summary>
    ///     Removes a source from the resolver.<br />
    ///     If the source was not added, this method does nothing.
    /// </summary>
    /// <param name="source">The source to remove.</param>
    /// <returns>The resolver instance, allowing calls to be chained.</returns>
    IResolver RemoveSource(IResolverSource source);

    /// <summary>
    ///     Resolves variables and functions within the given template text.
    /// </summary>
    /// <param name="input">The template text containing variables and/or functions to be resolved.</param>
    /// <returns>The resolved output text, or <see langword="null" /> if <paramref name="input" /> is <see langword="null" />.</returns>
    /// <remarks>
    ///     Variables are written as <c>{name}</c> and functions as <c>{name(args)}</c>.<br />
    ///     Braces can be escaped with a backslash (e.g., <c>\{notAVariable\}</c>) to exclude them from
    ///     resolution; the backslashes are preserved in the output.<br />
    ///     Resolution is recursive: if a resolved value itself contains variables or functions,
    ///     they are resolved as well.<br />
    ///     Placeholders that no source can resolve are left in the output unchanged.
    /// </remarks>
    /// <example>
    ///     The following example demonstrates how to use the Resolve method to resolve variables and functions:
    ///     <br />
    ///     <code lang="csharp">
    ///     resolverInstance.Resolve("The {color} {animal} jumps over the \\{escapedVariable\\} and calls {function()}.");
    ///     </code>
    ///     <br />
    ///     The output could look like: "The brown fox jumps over the \{escapedVariable\} and calls the-function-result."
    /// </example>
    string? Resolve(string? input);
}
