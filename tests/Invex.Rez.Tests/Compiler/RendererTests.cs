namespace Invex.Rez.Tests.Compiler;

[TestFixture]
public class RendererTests
{
    private static readonly Func<string, string> NoMatch = s => $"{{{s}}}";

    [Test]
    public void Render_Literal_Single()
    {
        const string inputText = "literal";
        Span<SyntaxToken> tokens = [SyntaxToken.CreateLiteral(0, inputText.Length)];
        Span<char> outputText = stackalloc char[inputText.Length];
        var resolution = new Func<string, string>(_ => "resolved");

        var count = Renderer.Render(tokens, inputText.AsSpan(), outputText, resolution);

        outputText[..count]
            .ToString()
            .ShouldBe(inputText);
    }

    [Test]
    public void Render_Literal_Multiple()
    {
        const string inputText = "literalliteralliteral";

        Span<SyntaxToken> tokens =
        [
            SyntaxToken.CreateLiteral(0, 7), SyntaxToken.CreateLiteral(7, 7), SyntaxToken.CreateLiteral(14, 7),
        ];

        Span<char> outputText = stackalloc char[inputText.Length * 3];
        var resolution = new Func<string, string>(_ => "resolved");

        var count = Renderer.Render(tokens, inputText.AsSpan(), outputText, resolution);

        outputText[..count]
            .ToString()
            .ShouldBe(inputText);
    }

    [Test]
    public void RenderVariable_Single()
    {
        const string inputText = "{var}";
        const string expectedText = "resolved";
        Span<SyntaxToken> tokens = [SyntaxToken.CreateLiteral(1, 3), SyntaxToken.CreateVariable(1, 3)];
        Span<char> outputText = stackalloc char[expectedText.Length];

        var resolution = new Func<string, string>(s => s == "var"
            ? "resolved"
            : s);

        var count = Renderer.Render(tokens, inputText.AsSpan(), outputText, resolution);

        outputText[..count]
            .ToString()
            .ShouldBe(expectedText);
    }

    [Test]
    public void RenderVariable_Multiple()
    {
        const string inputText = "{var}{var}{var}";
        const string expectedText = "resolved{var}{var}";

        Span<SyntaxToken> tokens =
        [
            SyntaxToken.CreateLiteral(1, 3),
            SyntaxToken.CreateVariable(1, 3),
            SyntaxToken.CreateLiteral(5, 3),
            SyntaxToken.CreateVariable(5, 3),
            SyntaxToken.CreateLiteral(9, 3),
            SyntaxToken.CreateVariable(9, 3),
        ];

        Span<char> outputText = stackalloc char[expectedText.Length];

        var resolution = new Func<string, string>(s => s == "var"
            ? "resolved"
            : s);

        var count = Renderer.Render(tokens, inputText.AsSpan(), outputText, resolution);

        outputText[..count]
            .ToString()
            .ShouldBe(expectedText);
    }

    [Test]
    public void RenderVariable_Nested()
    {
        const string inputText = "{var1{var2}}";
        const string expectedText = "{var1resolved}";

        Span<SyntaxToken> tokens =
        [
            SyntaxToken.CreateLiteral(6, 4),
            SyntaxToken.CreateVariable(6, 4),
            SyntaxToken.CreateLiteral(1, 10),
            SyntaxToken.CreateVariable(1, 10),
        ];

        Span<char> outputText = stackalloc char[expectedText.Length];

        var resolution = new Func<string, string>(s => s == "var2"
            ? "resolved"
            : s);

        var count = Renderer.Render(tokens, inputText.AsSpan(), outputText, resolution);

        outputText[..count]
            .ToString()
            .ShouldBe(expectedText);
    }

    [Test]
    public void RenderVariable_NoFirstMatch()
    {
        const string inputText = "{var1}{var2}";
        const string expectedText = "{var1}resolved";

        Span<SyntaxToken> tokens =
        [
            SyntaxToken.CreateLiteral(1, 4),
            SyntaxToken.CreateVariable(1, 4),
            SyntaxToken.CreateLiteral(7, 4),
            SyntaxToken.CreateVariable(7, 4),
        ];

        Span<char> outputText = stackalloc char[expectedText.Length];

        var resolution = new Func<string, string>(s => s == "var2"
            ? "resolved"
            : NoMatch(s));

        var count = Renderer.Render(tokens, inputText.AsSpan(), outputText, resolution);

        outputText[..count]
            .ToString()
            .ShouldBe(expectedText);
    }
}
