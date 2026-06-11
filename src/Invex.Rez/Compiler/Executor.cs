namespace Invex.Rez.Compiler;

/// <summary>
///     Drives the Rez compilation pipeline: lexing, tokenizing, and rendering the input repeatedly
///     until the output stabilizes (i.e., no further resolutions are possible).
/// </summary>
internal static class Executor
{
    /// <summary>
    ///     Executes the compilation pipeline over the given input.
    /// </summary>
    /// <param name="input">The template text to resolve.</param>
    /// <param name="resolution">
    ///     A callback that resolves a single placeholder's inner text (without braces) to its value,
    ///     or returns <see langword="null" /> if it cannot be resolved.
    /// </param>
    /// <returns>The fully resolved text, or <see langword="null" /> if <paramref name="input" /> is <see langword="null" />.</returns>
    /// <remarks>
    ///     Each pass resolves at most one placeholder; passes repeat until the text no longer changes.
    ///     This naturally handles nested and recursive resolutions.
    ///     A maximum depth guards against infinite recursion (e.g., a variable that resolves to itself).
    ///     Buffers are rented from <see cref="ArrayPool{T}" /> to avoid per-call allocations.
    /// </remarks>
    public static string? Execute(string? input, Func<string, string?> resolution)
    {
        if (input is null)
            return null;

        if (input.Length == 0)
            return string.Empty;

        var lexemeArray = ArrayPool<Lexeme>.Shared.Rent(4096);
        var lexemeBuffer = lexemeArray.AsSpan();

        var tokenArray = ArrayPool<SyntaxToken>.Shared.Rent(4096);
        var tokenBuffer = tokenArray.AsSpan();

        var primaryTextArray = ArrayPool<char>.Shared.Rent(4096);
        var primaryTextBuffer = primaryTextArray.AsSpan();
        var secondaryTextArray = ArrayPool<char>.Shared.Rent(4096);
        var secondaryTextBuffer = secondaryTextArray.AsSpan();

        var swapBuffer = false;

        input
            .AsSpan()
            .CopyTo(primaryTextBuffer);

        var inputLength = input.Length;
        int outputLength;

        var resolutionDepth = 0;

        while (true)
        {
            if (resolutionDepth > 4096)
                throw new("Resolution depth exceeded.");

            var sourceBuffer = swapBuffer
                ? secondaryTextBuffer
                : primaryTextBuffer;

            var destBuffer = swapBuffer
                ? primaryTextBuffer
                : secondaryTextBuffer;

            var lexemeLength = Lexer.Lex(sourceBuffer[..inputLength], lexemeBuffer);
            var tokenLength = Tokenizer.TokenizeLexemes(lexemeBuffer[..lexemeLength], tokenBuffer);

            outputLength = Renderer.Render(tokenBuffer[..tokenLength],
                sourceBuffer[..inputLength],
                destBuffer,
                resolution);

            swapBuffer = !swapBuffer;

            var render = false;

            if (outputLength != inputLength)
                render = true;
            else
                for (var i = 0; i < outputLength; i++)
                {
                    if (destBuffer[i] == sourceBuffer[i])
                        continue;

                    render = true;

                    break;
                }

            if (!render)
                break;

            inputLength = outputLength;
            resolutionDepth++;
        }

        var output = (swapBuffer
                ? secondaryTextBuffer
                : primaryTextBuffer)[..outputLength]
            .ToString();

        ArrayPool<char>.Shared.Return(primaryTextArray);
        ArrayPool<char>.Shared.Return(secondaryTextArray);
        ArrayPool<Lexeme>.Shared.Return(lexemeArray);
        ArrayPool<SyntaxToken>.Shared.Return(tokenArray);

        return output;
    }
}
