# API Reference

This section contains the generated reference documentation for every public type in the Rez
libraries, built from the XML documentation comments in the source code.

## Invex.Rez

The core templating library.

### Invex.Rez.Abstraction

| Type | Description |
| --- | --- |
| `IResolver` | Resolves variables and functions in a Rez template into an output string. |
| `IResolverSource` | Provides a map of variables and functions to be used by an `IResolver`. |
| `IResolverStore` | An `IResolverSource` that also supports adding and removing variables and functions at any time. |
| `FunctionCall` | Contains the raw argument text for a Rez function call. |

### Invex.Rez.Implementation

| Type | Description |
| --- | --- |
| `Resolver` | The default `IResolver` implementation. |
| `ResolverSource` | An immutable `IResolverSource` backed by fixed maps supplied at construction time. |
| `ResolverStore` | The default mutable `IResolverStore` implementation. |

## Invex.Rez.Configuration

Integration with `Microsoft.Extensions.Configuration` and dependency injection.

| Type | Description |
| --- | --- |
| `ConfigResolverSource` | An `IResolverSource` that resolves variables from an `IConfigurationRoot`. |
| `IResolvableConfig` | An `IConfigurationRoot` whose values are resolved with an `IResolver` on read. |
| `Setup` | Extension methods for registering Rez configuration services with dependency injection. |

Use the navigation tree to browse the full reference for each namespace, type, and member.

