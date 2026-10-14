namespace Invex.Rez.Compiler;

/// <summary>
///     Renders tokenized template text by substituting the first resolvable variable token with its value.
/// </summary>
internal static class Renderer
{
    /// <summary>
    ///     Renders the input text into <paramref name="outputText" />, substituting at most one variable token.
    /// </summary>
    /// <param name="tokens">The syntax tokens describing the input text.</param>
    /// <param name="inputText">The text the tokens refer to.</param>
    /// <param name="outputText">The buffer that receives the rendered text.</param>
    /// <param name="resolution">
    ///     A callback that resolves a placeholder's inner text (without braces) to its value,
    ///     or returns <see langword="null" /> if it cannot be resolved.
    /// </param>
    /// <returns>The length of the rendered text written to <paramref name="outputText" />.</returns>
    /// <remarks>
    ///     Only the first variable token whose resolution differs from its original placeholder text
    ///     is substituted per call; the <see cref="Executor" /> invokes this repeatedly until the text stabilizes.
    ///     Unresolvable placeholders are left intact.
    /// </remarks>
    public static int Render(
        Span<SyntaxToken> tokens,
        ReadOnlySpan<char> inputText,
        Span<char> outputText,
        Func<string, string?> resolution)
    {
        if (!TryFindSubstitution(tokens, inputText, resolution, out var variableStart, out var variableLength, out var value))
        {
            inputText.CopyTo(outputText);

            return inputText.Length;
        }

        return Substitute(inputText, variableStart, variableLength, value, outputText);
    }

    /// <summary>
    ///     Finds the first variable token whose resolution differs from its original placeholder text.
    /// </summary>
    /// <param name="tokens">The syntax tokens describing the input text.</param>
    /// <param name="inputText">The text the tokens refer to.</param>
    /// <param name="resolution">
    ///     A callback that resolves a placeholder's inner text (without braces) to its value,
    ///     or returns <see langword="null" /> if it cannot be resolved.
    /// </param>
    /// <param name="variableStart">The index of the placeholder's opening brace in <paramref name="inputText" />.</param>
    /// <param name="variableLength">The length of the placeholder, including both braces.</param>
    /// <param name="value">The value that replaces the placeholder.</param>
    /// <returns><see langword="true" /> if a substitution was found; otherwise <see langword="false" />.</returns>
    public static bool TryFindSubstitution(
        Span<SyntaxToken> tokens,
        ReadOnlySpan<char> inputText,
        Func<string, string?> resolution,
        out int variableStart,
        out int variableLength,
        out string value)
    {
        foreach (var token in tokens)
        {
            if (token.Type != SyntaxToken.Variable)
                continue;

            var start = token.Start - 1;
            var length = token.Length + 2;

            var variable = inputText
                .Slice(start, length)
                .ToString();

            var resolutionInput = inputText
                .Slice(token.Start, token.Length)
                .ToString();

            var resolutionResult = resolution(resolutionInput) ?? variable;

            if (resolutionResult == variable)
                continue;

            variableStart = start;
            variableLength = length;
            value = resolutionResult;

            return true;
        }

        variableStart = 0;
        variableLength = 0;
        value = string.Empty;

        return false;
    }

    /// <summary>
    ///     Writes <paramref name="inputText" /> to <paramref name="outputText" /> with the placeholder at
    ///     <paramref name="variableStart" /> replaced by <paramref name="value" />.
    /// </summary>
    /// <param name="inputText">The text containing the placeholder.</param>
    /// <param name="variableStart">The index of the placeholder's opening brace.</param>
    /// <param name="variableLength">The length of the placeholder, including both braces.</param>
    /// <param name="value">The value that replaces the placeholder.</param>
    /// <param name="outputText">
    ///     The buffer that receives the rendered text; it must hold at least
    ///     <c>inputText.Length - variableLength + value.Length</c> characters.
    /// </param>
    /// <returns>The length of the rendered text written to <paramref name="outputText" />.</returns>
    public static int Substitute(
        ReadOnlySpan<char> inputText,
        int variableStart,
        int variableLength,
        string value,
        Span<char> outputText)
    {
        inputText[..variableStart]
            .CopyTo(outputText);

        value
            .AsSpan()
            .CopyTo(outputText[variableStart..]);

        inputText[(variableStart + variableLength)..]
            .CopyTo(outputText[(variableStart + value.Length)..]);

        return inputText.Length - variableLength + value.Length;
    }
}
