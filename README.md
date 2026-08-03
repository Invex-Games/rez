# Rez

Rez is a small .NET library for resolving terse, nestable template text such as `{name}` and
`{function(args)}`. It is designed for application strings, configuration values, and other text
where a full template engine would be unnecessary.

Rez takes inspiration from [Mustache](https://mustache.github.io/), but uses curly-brace tokens that
can be nested and resolved recursively.

```csharp
var resolver = new Resolver();
resolver.AddSource(new ResolverSource(new Dictionary<string, string> { { "name", "World" } }));

var greeting = resolver.Resolve("Hello, {name}!");

// greeting: "Hello, World!"
```

## Features and behavior

- **Variables** — `{name}` is replaced with its value
- **Functions** — `{repeat(abc,3)}` invokes a function with the raw argument text
- **Nesting** — `{variable{number}}` resolves inside-out, left-to-right
- **Recursion** — resolved values may themselves contain placeholders, with a 4096-pass limit
- **Escaping** — `\{literal\}` excludes braces from resolution (the backslashes are preserved in the output)
- **Pluggable sources** — layer any number of variable/function sources; the first non-null result wins
- **Graceful fallback** — unresolvable placeholders are left in the output unchanged
- **High performance** — pooled buffers and span-based parsing minimize allocations
- **Configuration integration** — resolve templates inside `Microsoft.Extensions.Configuration`
  values

`Resolve(null)` returns `null`. Missing variables and functions do not throw. Function delegates must
return a non-null string; a source returns `null` only when it cannot resolve a name and wants the
next source to be queried.

## Installation

Install the core package:

```shell
dotnet add package Invex.Rez
```

For `Microsoft.Extensions.Configuration` integration, install the additional package:

```shell
dotnet add package Invex.Rez.Configuration
```

Both packages target `net10.0`, `net9.0`, `net8.0`, and `netstandard2.0`. The test project also
runs on `net48`.

## Variables and functions

Sources provide variables, functions, or both. `ResolverSource` is immutable and initialized with
known values; `ResolverStore` is mutable and replaces an existing entry when the same name is added.

```csharp
var resolver = new Resolver();
resolver.AddSource(new ResolverSource(new Dictionary<string, string>
{
    ["name"] = "World"
}));

var greeting = resolver.Resolve("Hello, {name}!");

// greeting: "Hello, World!"
```

Functions receive a `FunctionCall` whose `Args` property contains all text between the parentheses.
Rez does not split or trim arguments; parsing is owned by the function:

```csharp
var resolver = new Resolver()
    .AddSource(new ResolverSource(new Dictionary<string, Func<FunctionCall, string>>
    {
        ["shout"] = call => (call.Args ?? string.Empty).ToUpperInvariant() + "!"
    }));

var output = resolver.Resolve("{shout(hello)}");

// output: "HELLO!"
```

## Source ordering and mutable values

Sources are queried in registration order for every placeholder. The first non-null result wins, so
register overrides before defaults:

```csharp
var overrides = new ResolverStore();
var defaults = new ResolverSource(new Dictionary<string, string> { ["env"] = "production" });

var resolver = new Resolver()
    .AddSource(overrides)
    .AddSource(defaults);

resolver.Resolve("{env}"); // "production"
overrides.AddVariable("env", "development");
resolver.Resolve("{env}"); // "development"
```

Use `RemoveVariable` and `RemoveFunction` to remove entries from a `ResolverStore`. Resolver and
source types are not synchronized; configure them before concurrent use, or provide synchronization
when mutating them.

## Configuration integration

`Invex.Rez.Configuration` provides two related features:

1. `ConfigResolverSource` exposes `IConfigurationRoot` values as Rez variables, including nested
   keys such as `{Logging:Level}`.
2. `IResolvableConfig` wraps configuration and resolves templates when values are read. The
   `AddResolvableConfiguration()` extension registers it in dependency injection.

```csharp
var config = (IConfigurationRoot)new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var resolver = new Resolver()
    .AddSource(new ConfigResolverSource(config));

var output = resolver.Resolve("The {ServiceName} service is ready.");
```

Values written through `IResolvableConfig` are stored unmodified; resolution occurs on read.
See the [Configuration Integration](docs/configuration.md) guide for dependency-injection setup.

## Development

The repository requires the .NET 10 SDK selected by `global.json`:

```shell
dotnet build Invex.Rez.slnx
dotnet test Invex.Rez.slnx
```

See [AGENTS.md](AGENTS.md) for repository structure, code conventions, generated workflow rules,
snapshot testing, and the complete contributor checklist.

## Documentation

- [Introduction](docs/index.md) — what Rez is and why it exists
- [Getting Started](docs/getting-started.md) — install and resolve your first template
- [Syntax Guide](docs/syntax.md) — the complete template language reference
- [Examples](docs/examples.md) — worked examples from simple to advanced
- [Developer Guide](docs/developer-guide.md) — abstractions, custom sources, and how resolution works
- [Configuration Integration](docs/configuration.md) — resolving templates in app configuration
- [API Reference](api/index.md) — generated reference for every public type

## Where did the name come from?

The name Rez is a play on the word "res" (short for "resolution"), which is the process of
resolving a template. The name was chosen as it is short and memorable and is not too similar to
other names in the .NET ecosystem.

## License

See [LICENSE.txt](LICENSE.txt).
