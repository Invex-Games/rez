namespace Invex.Rez.Tests.Compiler;

[TestFixture]
public class TokenizerTests
{
    [Test]
    public void Tokenize_Literal_Single()
    {
        Span<Lexeme> input = stackalloc Lexeme[1];
        input[0] = Lexeme.CreateLiteral(0, 1);

        Span<SyntaxToken> output = stackalloc SyntaxToken[1];

        var count = Tokenizer.TokenizeLexemes(input, output);

        count.ShouldBe(1);

        output[0]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[0]
            .Start
            .ShouldBe(0);

        output[0]
            .Length
            .ShouldBe(1);
    }

    [Test]
    public void Tokenize_Literal_Multiple()
    {
        Span<Lexeme> input = stackalloc Lexeme[1];
        input[0] = Lexeme.CreateLiteral(0, 10);

        Span<SyntaxToken> output = stackalloc SyntaxToken[1];

        var count = Tokenizer.TokenizeLexemes(input, output);

        count.ShouldBe(1);

        output[0]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[0]
            .Start
            .ShouldBe(0);

        output[0]
            .Length
            .ShouldBe(10);
    }

    [Test]
    public void Tokenize_OpenBrace_Single()
    {
        Span<Lexeme> input = stackalloc Lexeme[1];
        input[0] = Lexeme.CreateOpenBrace(0);

        Span<SyntaxToken> output = stackalloc SyntaxToken[1];

        var count = Tokenizer.TokenizeLexemes(input, output);

        count.ShouldBe(1);

        output[0]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[0]
            .Start
            .ShouldBe(0);

        output[0]
            .Length
            .ShouldBe(1);
    }

    [Test]
    public void Tokenize_OpenBrace_Multiple_Returns_Literal()
    {
        Span<Lexeme> input = stackalloc Lexeme[3];
        input[0] = Lexeme.CreateOpenBrace(0);
        input[1] = Lexeme.CreateOpenBrace(1);
        input[2] = Lexeme.CreateOpenBrace(2);

        Span<SyntaxToken> output = stackalloc SyntaxToken[3];

        var count = Tokenizer.TokenizeLexemes(input, output);

        count.ShouldBe(1);

        output[0]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[0]
            .Start
            .ShouldBe(0);

        output[0]
            .Length
            .ShouldBe(3);
    }

    [Test]
    public void Tokenize_OpenBrace_Multiple_With_Variable()
    {
        Span<Lexeme> input = stackalloc Lexeme[5];
        input[0] = Lexeme.CreateOpenBrace(0);
        input[1] = Lexeme.CreateOpenBrace(1);
        input[2] = Lexeme.CreateOpenBrace(2);
        input[3] = Lexeme.CreateLiteral(3, 1);
        input[4] = Lexeme.CreateCloseBrace(4);

        Span<SyntaxToken> output = stackalloc SyntaxToken[5];

        var count = Tokenizer.TokenizeLexemes(input, output);

        count.ShouldBe(2);

        output[0]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[0]
            .Start
            .ShouldBe(3);

        output[0]
            .Length
            .ShouldBe(1);

        output[1]
            .Type
            .ShouldBe(SyntaxToken.Variable);

        output[1]
            .Start
            .ShouldBe(3);

        output[1]
            .Length
            .ShouldBe(1);
    }

    [Test]
    public void Tokenize_CloseBrace_Single()
    {
        Span<Lexeme> input = stackalloc Lexeme[1];
        input[0] = Lexeme.CreateCloseBrace(0);

        Span<SyntaxToken> output = stackalloc SyntaxToken[1];

        var count = Tokenizer.TokenizeLexemes(input, output);

        count.ShouldBe(1);

        output[0]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[0]
            .Start
            .ShouldBe(0);

        output[0]
            .Length
            .ShouldBe(1);
    }

    [Test]
    public void Tokenize_CloseBrace_Multiple_Returns_Literals()
    {
        Span<Lexeme> input = stackalloc Lexeme[3];
        input[0] = Lexeme.CreateCloseBrace(0);
        input[1] = Lexeme.CreateCloseBrace(1);
        input[2] = Lexeme.CreateCloseBrace(2);

        Span<SyntaxToken> output = stackalloc SyntaxToken[3];

        var count = Tokenizer.TokenizeLexemes(input, output);

        count.ShouldBe(3);

        output[0]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[0]
            .Start
            .ShouldBe(0);

        output[0]
            .Length
            .ShouldBe(1);

        output[1]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[1]
            .Start
            .ShouldBe(1);

        output[1]
            .Length
            .ShouldBe(1);

        output[2]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[2]
            .Start
            .ShouldBe(2);

        output[2]
            .Length
            .ShouldBe(1);
    }

    [Test]
    public void Tokenize_CloseBrace_Multiple_With_Variable()
    {
        Span<Lexeme> input = stackalloc Lexeme[5];
        input[0] = Lexeme.CreateCloseBrace(0);
        input[1] = Lexeme.CreateCloseBrace(1);
        input[2] = Lexeme.CreateOpenBrace(2);
        input[3] = Lexeme.CreateLiteral(3, 1);
        input[4] = Lexeme.CreateCloseBrace(4);

        Span<SyntaxToken> output = stackalloc SyntaxToken[5];

        var count = Tokenizer.TokenizeLexemes(input, output);

        count.ShouldBe(4);

        output[0]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[0]
            .Start
            .ShouldBe(0);

        output[0]
            .Length
            .ShouldBe(1);

        output[1]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[1]
            .Start
            .ShouldBe(1);

        output[1]
            .Length
            .ShouldBe(1);

        output[2]
            .Type
            .ShouldBe(SyntaxToken.Literal);

        output[2]
            .Start
            .ShouldBe(3);

        output[2]
            .Length
            .ShouldBe(1);

        output[3]
            .Type
            .ShouldBe(SyntaxToken.Variable);

        output[3]
            .Start
            .ShouldBe(3);

        output[3]
            .Length
            .ShouldBe(1);
    }
}
