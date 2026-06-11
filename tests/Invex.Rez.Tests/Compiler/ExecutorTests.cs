namespace Invex.Rez.Tests.Compiler;

[TestFixture]
[SuppressMessage("ReSharper", "ConvertToLocalFunction")]
public class ExecutorTests
{
    private static readonly Func<string, string> NoMatch = s => $"{{{s}}}";

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("example")]
    [TestCase("{example}")]
    [TestCase("{{example}}")]
    [TestCase("{{example()}}")]
    [TestCase("{{example(something)}}")]
    [TestCase("{{{example}}}")]
    [TestCase("{{{example(something)}}}")]
    [TestCase(@"\{{{example\}}}")]
    [TestCase(@"\{{{example(something)\}}}")]
    public void Execute_NoMatch_Returns_Input(string? input)
    {
        var result = Executor.Execute(input, NoMatch);

        result.ShouldBe(input);
    }

    [TestCase("{example}", "example", "value", "value")]
    [TestCase("{example}", "example", "{example}", "{example}")]
    [TestCase("{{example}}", "example", "value", "{value}")]
    [TestCase("{example}", "example", "{{value}}", "{{value}}")]
    [TestCase(@"\{example\}", "example", "value", @"\{example\}")]
    [TestCase(@"\{{example\}}", "example\\}", "value", @"\{value")]
    public void Resolve_VariableReference_Returns_Value(string input, string key, string value, string expected)
    {
        var result = Executor.Execute(input,
            s => s == key
                ? value
                : NoMatch(s));

        result.ShouldBe(expected);
    }

    [Test]
    public void Resolve_MultipleVariables_Returns_Value()
    {
        Func<string, string> resolution = s => s switch
        {
            "variableName1" => "value1",
            "variableName2" => "value2",
            _ => NoMatch(s),
        };

        var result = Executor.Execute("prefix_{variableName1}_middle_{variableName2}_postfix", resolution);

        result.ShouldBe("prefix_value1_middle_value2_postfix");
    }

    [Test]
    public void Resolve_NestedVariable_Returns_Value()
    {
        Func<string, string> resolution = s => s switch
        {
            "key1" => "{key2}",
            "key2" => "{key3}",
            "key3" => "value",
            _ => NoMatch(s),
        };

        var result = Executor.Execute("{key1}", resolution);

        result.ShouldBe("value");
    }
}
