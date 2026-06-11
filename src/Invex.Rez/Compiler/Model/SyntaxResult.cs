namespace Invex.Rez.Compiler.Model;

/// <summary>
///     The result of a single <see cref="Tokenizer" /> step, used to advance through the lexeme and token buffers.
/// </summary>
/// <param name="ConsumedLexemes">The number of lexemes consumed by the step.</param>
/// <param name="ProducedTokens">The number of tokens produced by the step.</param>
internal readonly record struct SyntaxResult(int ConsumedLexemes, int ProducedTokens);
