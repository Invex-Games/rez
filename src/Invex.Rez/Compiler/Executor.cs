namespace Invex.Rez.Compiler;

/// <summary>
///     Drives the Rez compilation pipeline: lexing, tokenizing, and rendering the input repeatedly
///     until the output stabilizes (i.e., no further resolutions are possible).
/// </summary>
internal static class Executor
{
    /// <summary>
    ///     The maximum number of substitution passes before resolution is abandoned.
    /// </summary>
    internal const int MaxResolutionDepth = 4096;

    /// <summary>
    ///     Executes the compilation pipeline over the given input.
    /// </summary>
    /// <param name="input">The template text to resolve.</param>
    /// <param name="resolution">
    ///     A callback that resolves a single placeholder's inner text (without braces) to its value,
    ///     or returns <see langword="null" /> if it cannot be resolved.
    /// </param>
    /// <returns>The fully resolved text, or <see langword="null" /> if <paramref name="input" /> is <see langword="null" />.</returns>
    /// <exception cref="RezResolutionException">
    ///     Thrown if the text is still changing after <see cref="MaxResolutionDepth" /> passes, such as for a cycle
    ///     like <c>a = {b}</c>, <c>b = {a}</c>.
    /// </exception>
    /// <remarks>
    ///     Each pass resolves at most one placeholder; passes repeat until the text no longer changes.
    ///     This naturally handles nested and recursive resolutions.
    ///     A placeholder whose value is its own placeholder text (e.g., <c>a = {a}</c>) leaves the text unchanged,
    ///     so resolution simply stops there. A maximum depth guards against cycles whose output keeps changing
    ///     (e.g., <c>a = {b}</c>, <c>b = {a}</c>, or a value that contains its own placeholder plus other text).
    ///     Buffers are rented from <see cref="ArrayPool{T}" /> and grown as needed, so templates and resolved
    ///     values of any length are supported without per-pass allocations.
    /// </remarks>
    public static string? Execute(string? input, Func<string, string?> resolution)
    {
        if (input is null)
            return null;

        if (input.Length == 0)
            return string.Empty;

        // Each character produces at most one lexeme, and each lexeme at most one token.
        var lexemeArray = ArrayPool<Lexeme>.Shared.Rent(input.Length);
        var tokenArray = ArrayPool<SyntaxToken>.Shared.Rent(input.Length);
        var sourceArray = ArrayPool<char>.Shared.Rent(input.Length);
        var destArray = ArrayPool<char>.Shared.Rent(input.Length);

        try
        {
            input
                .AsSpan()
                .CopyTo(sourceArray);

            var textLength = input.Length;
            var resolutionDepth = 0;

            while (true)
            {
                if (resolutionDepth > MaxResolutionDepth)
                    throw new RezResolutionException("Resolution depth exceeded.");

                if (lexemeArray.Length < textLength)
                {
                    ArrayPool<Lexeme>.Shared.Return(lexemeArray);
                    lexemeArray = ArrayPool<Lexeme>.Shared.Rent(textLength);
                }

                if (tokenArray.Length < textLength)
                {
                    ArrayPool<SyntaxToken>.Shared.Return(tokenArray);
                    tokenArray = ArrayPool<SyntaxToken>.Shared.Rent(textLength);
                }

                var sourceText = sourceArray.AsSpan(0, textLength);

                var lexemeLength = Lexer.Lex(sourceText, lexemeArray);
                var tokenLength = Tokenizer.TokenizeLexemes(lexemeArray.AsSpan(0, lexemeLength), tokenArray);

                if (!Renderer.TryFindSubstitution(tokenArray.AsSpan(0, tokenLength),
                        sourceText,
                        resolution,
                        out var variableStart,
                        out var variableLength,
                        out var value))
                    break;

                var outputLength = textLength - variableLength + value.Length;

                if (destArray.Length < outputLength)
                {
                    ArrayPool<char>.Shared.Return(destArray);
                    destArray = ArrayPool<char>.Shared.Rent(outputLength);
                }

                textLength = Renderer.Substitute(sourceText, variableStart, variableLength, value, destArray);

                (sourceArray, destArray) = (destArray, sourceArray);
                resolutionDepth++;
            }

            return new(sourceArray, 0, textLength);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(sourceArray);
            ArrayPool<char>.Shared.Return(destArray);
            ArrayPool<Lexeme>.Shared.Return(lexemeArray);
            ArrayPool<SyntaxToken>.Shared.Return(tokenArray);
        }
    }
}
