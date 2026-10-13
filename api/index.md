# API Reference

This section contains the generated reference for every public type in the Rez packages, built from
the XML documentation comments in the source code.

## Invex.Rez

The core templating library.

### Invex.Rez.Abstraction

| Type | Kind | Description |
| --- | --- | --- |
| [`IResolver`](xref:Invex.Rez.Abstraction.IResolver) | Interface | Resolves variables and functions in a Rez template into an output string. |
| [`IResolverSource`](xref:Invex.Rez.Abstraction.IResolverSource) | Interface | Provides a map of variables and functions to be used by an `IResolver`. |
| [`IResolverStore`](xref:Invex.Rez.Abstraction.IResolverStore) | Interface | An `IResolverSource` that also supports adding and removing variables and functions at any time. |
| [`FunctionCall`](xref:Invex.Rez.Abstraction.FunctionCall) | Record struct | Contains the raw argument text for a Rez function call. |

### Invex.Rez.Implementation

| Type | Kind | Description |
| --- | --- | --- |
| [`Resolver`](xref:Invex.Rez.Implementation.Resolver) | Class | The default `IResolver` implementation. |
| [`ResolverSource`](xref:Invex.Rez.Implementation.ResolverSource) | Class | An immutable `IResolverSource` backed by fixed maps supplied at construction time. |
| [`ResolverStore`](xref:Invex.Rez.Implementation.ResolverStore) | Class | The default mutable `IResolverStore` implementation. |

## Invex.Rez.Configuration

Integration with `Microsoft.Extensions.Configuration` and dependency injection.

| Type | Kind | Description |
| --- | --- | --- |
| [`ConfigResolverSource`](xref:Invex.Rez.Configuration.ConfigResolverSource) | Class | An `IResolverSource` that resolves variables from an `IConfigurationRoot`. |
| [`IResolvableConfig`](xref:Invex.Rez.Configuration.IResolvableConfig) | Interface | An `IConfigurationRoot` whose values are resolved with an `IResolver` on read. |
| [`Setup`](xref:Invex.Rez.Configuration.Setup) | Static class | Extension methods for registering Rez configuration services with dependency injection (`AddResolvableConfiguration`). |

## Typical flow

```csharp
var resolver = new Resolver()
    .AddSource(new ResolverStore())                                   // runtime overrides
    .AddSource(new ResolverSource(new Dictionary<string, string>      // fixed defaults
    {
        ["env"] = "production"
    }));

var text = resolver.Resolve("Deploying to {env}");
```

For concepts and guides, start with the [Introduction](../docs/introduction.md).
