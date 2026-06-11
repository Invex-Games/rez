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
        foreach (var token in tokens)
        {
            if (token.Type != SyntaxToken.Variable)
                continue;

            var variableStart = token.Start - 1;
            var variableLength = token.Length + 2;

            var variable = inputText
                .Slice(variableStart, variableLength)
                .ToString();

            var resolutionInput = inputText
                .Slice(token.Start, token.Length)
                .ToString();

            var resolutionResult = resolution(resolutionInput) ?? variable;

            if (resolutionResult == variable)
                continue;

            inputText[..variableStart]
                .CopyTo(outputText);

            resolutionResult
                .AsSpan()
                .CopyTo(outputText[variableStart..]);

            inputText[(variableStart + variableLength)..]
                .CopyTo(outputText[(variableStart + resolutionResult.Length)..]);

            return variableStart + resolutionResult.Length + inputText.Length - (variableStart + variableLength);
        }

        inputText.CopyTo(outputText);

        return inputText.Length;
    }
}
