namespace Invex.Rez.Compiler;

/// <summary>
///     Drives the Rez compilation pipeline: lexing, tokenizing, and rendering the input repeatedly
///     until the output stabilizes (i.e., no further resolutions are possible).
/// </summary>
internal static class Executor
{
    /// <summary>
    ///     The maximum substitution depth: how many times text produced by resolving a placeholder may itself
    ///     contain a placeholder that is resolved in turn (e.g., <c>{a}</c> to <c>{b}</c> to <c>{c}</c> is depth 3).
    ///     Placeholders written in the template itself are depth 1, however many there are.
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
    ///     Thrown if a substitution would exceed <see cref="MaxResolutionDepth" />, such as for a cycle like
    ///     <c>a = {b}</c>, <c>b = {a}</c>, or a value that contains its own placeholder plus other text.
    /// </exception>
    /// <remarks>
    ///     Each pass resolves at most one placeholder; passes repeat until the text no longer changes.
    ///     This naturally handles nested and recursive resolutions.
    ///     A placeholder whose value is its own placeholder text (e.g., <c>a = {a}</c>) leaves the text unchanged,
    ///     so resolution simply stops there.
    ///     <br />
    ///     Every character carries the depth of the substitution that produced it (0 for template text).
    ///     A placeholder resolves at one more than the deepest character it spans, and its value's characters
    ///     take that depth. Runaway resolution (a changing cycle or an ever-growing expansion) therefore keeps
    ///     getting deeper and hits <see cref="MaxResolutionDepth" />, while any number of independent
    ///     placeholders in a template or value does not count against the limit.
    ///     <br />
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

        // The substitution depth of each character in the source / dest text.
        var sourceDepthArray = ArrayPool<int>.Shared.Rent(input.Length);
        var destDepthArray = ArrayPool<int>.Shared.Rent(input.Length);

        try
        {
            input
                .AsSpan()
                .CopyTo(sourceArray);

            sourceDepthArray
                .AsSpan(0, input.Length)
                .Clear();

            var textLength = input.Length;

            while (true)
            {
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
                var sourceDepths = sourceDepthArray.AsSpan(0, textLength);

                var lexemeLength = Lexer.Lex(sourceText, lexemeArray);
                var tokenLength = Tokenizer.TokenizeLexemes(lexemeArray.AsSpan(0, lexemeLength), tokenArray);

                if (!Renderer.TryFindSubstitution(tokenArray.AsSpan(0, tokenLength),
                        sourceText,
                        resolution,
                        out var variableStart,
                        out var variableLength,
                        out var value))
                    break;

                var depth = 0;

                foreach (var characterDepth in sourceDepths.Slice(variableStart, variableLength))
                    if (characterDepth > depth)
                        depth = characterDepth;

                depth++;

                if (depth > MaxResolutionDepth)
                    throw new RezResolutionException("Resolution depth exceeded.");

                var outputLength = textLength - variableLength + value.Length;

                if (destArray.Length < outputLength)
                {
                    ArrayPool<char>.Shared.Return(destArray);
                    destArray = ArrayPool<char>.Shared.Rent(outputLength);
                }

                if (destDepthArray.Length < outputLength)
                {
                    ArrayPool<int>.Shared.Return(destDepthArray);
                    destDepthArray = ArrayPool<int>.Shared.Rent(outputLength);
                }

                textLength = Renderer.Substitute(sourceText, variableStart, variableLength, value, destArray);

                var destDepths = destDepthArray.AsSpan(0, textLength);
                sourceDepths[..variableStart].CopyTo(destDepths);
                destDepths.Slice(variableStart, value.Length).Fill(depth);
                sourceDepths[(variableStart + variableLength)..].CopyTo(destDepths[(variableStart + value.Length)..]);

                (sourceArray, destArray) = (destArray, sourceArray);
                (sourceDepthArray, destDepthArray) = (destDepthArray, sourceDepthArray);
            }

            return new(sourceArray, 0, textLength);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(sourceArray);
            ArrayPool<char>.Shared.Return(destArray);
            ArrayPool<int>.Shared.Return(sourceDepthArray);
            ArrayPool<int>.Shared.Return(destDepthArray);
            ArrayPool<Lexeme>.Shared.Return(lexemeArray);
            ArrayPool<SyntaxToken>.Shared.Return(tokenArray);
        }
    }
}
