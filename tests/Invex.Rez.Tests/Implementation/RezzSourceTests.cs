namespace Invex.Rez.Tests.Implementation;

[TestFixture]
public class RezzSourceTests
{
    [Test]
    public void Constructor_NullSource_Throws()
    {
        Should.Throw<Exception>(() => new ResolverSource(null!, new()));

        Should.Throw<Exception>(() => new ResolverSource(new(), null!));

        Should.Throw<Exception>(() => new ResolverSource(null!, []));

        Should.Throw<Exception>(() => new ResolverSource([], null!));

        Should.Throw<Exception>(() => new ResolverSource((IEnumerable<KeyValuePair<string, string>>)null!, null!));

        Should.Throw<Exception>(() => new ResolverSource((Dictionary<string, string>)null!));

        Should.Throw<Exception>(() => new ResolverSource((Dictionary<string, Func<FunctionCall, string>>)null!));

        Should.Throw<Exception>(() => new ResolverSource((IEnumerable<KeyValuePair<string, string>>)null!));

        Should.Throw<Exception>(() =>
            new ResolverSource((IEnumerable<KeyValuePair<string, Func<FunctionCall, string>>>)null!));
    }

    [Test]
    public void Constructor_Creates_Instance()
    {
        new ResolverSource(new(), new()).ShouldNotBeNull();

        new ResolverSource(new(), []).ShouldNotBeNull();

        new ResolverSource([], new()).ShouldNotBeNull();

        new ResolverSource([], []).ShouldNotBeNull();

        new ResolverSource(new Dictionary<string, string>()).ShouldNotBeNull();

        new ResolverSource(new Dictionary<string, Func<FunctionCall, string>>()).ShouldNotBeNull();

        new ResolverSource(Enumerable.Empty<KeyValuePair<string, string>>()).ShouldNotBeNull();

        new ResolverSource(Enumerable.Empty<KeyValuePair<string, Func<FunctionCall, string>>>()).ShouldNotBeNull();
    }

    [Test]
    public void ResolveVariable_NoSource_ReturnsNull()
    {
        var source = new ResolverSource(new Dictionary<string, string>());

        var result = source.ResolveVariable("var");

        result.ShouldBeNull();
    }

    [Test]
    public void ResolveVariable_Source_ReturnsValue()
    {
        var source = new ResolverSource(new Dictionary<string, string>
        {
            { "var", "value" },
        });

        var result = source.ResolveVariable("var");

        result.ShouldBe("value");
    }

    [Test]
    public void ResolveFunction_NoSource_ReturnsNull()
    {
        var source = new ResolverSource(new Dictionary<string, Func<FunctionCall, string>>());

        var result = source.ResolveFunction("func");

        result.ShouldBeNull();
    }

    [Test]
    public void ResolveFunction_Source_ReturnsValue()
    {
        var source = new ResolverSource(new Dictionary<string, Func<FunctionCall, string>>
        {
            { "func", _ => "value" },
        });

        var result = source.ResolveFunction("func");

        result.ShouldNotBeNull();

        result(new(string.Empty))
            .ShouldBe("value");
    }
}
