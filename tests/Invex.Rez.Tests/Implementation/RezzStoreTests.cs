namespace Invex.Rez.Tests.Implementation;

[TestFixture]
public class RezzStoreTests
{
    [Test]
    public void Constructor_Creates_Instance() =>
        new ResolverStore().ShouldNotBeNull();

    [TestCase("var", "value")]
    [TestCase("section:var", "value")]
    public void Add_Resolve_Variable_Returns_Value(string name, string value)
    {
        var store = new ResolverStore();
        store.AddVariable(name, value);

        store
            .ResolveVariable(name)
            .ShouldBe(value);
    }

    [TestCase("var", "value")]
    [TestCase("section:var", "value")]
    public void Add_Remove_Resolve_Variable_Returns_Null(string name, string value)
    {
        var store = new ResolverStore();
        store.AddVariable(name, value);
        store.RemoveVariable(name);

        store
            .ResolveVariable(name)
            .ShouldBeNull();
    }

    [TestCase("func", "value")]
    [TestCase("section:func", "value")]
    public void Add_Resolve_Function_Returns_Value(string name, string value)
    {
        var store = new ResolverStore();
        store.AddFunction(name, _ => value);

        store
            .ResolveFunction(name)
            ?.Invoke(new())
            .ShouldBe(value);
    }

    [TestCase("func", "value")]
    [TestCase("section:func", "value")]
    public void Add_Remove_Resolve_Function_Returns_Null(string name, string value)
    {
        var store = new ResolverStore();
        store.AddFunction(name, _ => value);
        store.RemoveFunction(name);

        store
            .ResolveFunction(name)
            .ShouldBeNull();
    }
}
