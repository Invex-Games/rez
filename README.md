# Rez

[![Build](https://github.com/Invex-Games/rez/actions/workflows/Build.yml/badge.svg)](https://github.com/Invex-Games/rez/actions/workflows/Build.yml)
[![NuGet](https://img.shields.io/nuget/v/Invex.Rez.svg)](https://www.nuget.org/packages/Invex.Rez)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Invex-Games/rez/blob/main/LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%20Standard%202.0-512BD4.svg)](https://dotnet.microsoft.com/)

Rez is a small .NET library for resolving terse, nestable template text such as `{name}` and
`{function(args)}`. It is designed for application strings, configuration values, and other text
where a full template engine would be unnecessary. Rez takes inspiration from
[Mustache](https://mustache.github.io/), but uses curly-brace tokens that can be nested and
resolved recursively.

```csharp
var resolver = new Resolver();
resolver.AddSource(new ResolverSource(new Dictionary<string, string> { { "name", "World" } }));

var greeting = resolver.Resolve("Hello, {name}!");

// greeting: "Hello, World!"
```

## Packages

| Package | What it does |
| --- | --- |
| [Invex.Rez](https://www.nuget.org/packages/Invex.Rez) | The core templating library: `Resolver`, `ResolverSource`, and `ResolverStore`. |
| [Invex.Rez.Configuration](https://www.nuget.org/packages/Invex.Rez.Configuration) | Integration with `Microsoft.Extensions.Configuration` and dependency injection. |

## Features

- **Variables.** `{name}` is replaced with its value.
- **Functions.** `{repeat(abc,3)}` invokes a function with the raw argument text.
- **Nesting.** `{variable{number}}` resolves inside-out, left-to-right.
- **Recursion.** Resolved values may themselves contain placeholders, with a 4096-pass limit.
- **Escaping.** `\{literal\}` excludes braces from resolution (the backslashes are preserved in the output).
- **Pluggable sources.** Layer any number of variable and function sources; the first non-null result wins.
- **Graceful fallback.** Unresolvable placeholders are left in the output unchanged, and `Resolve(null)` returns `null`.
- **High performance.** Pooled buffers and span-based parsing minimize allocations.
- **Configuration integration.** Resolve templates inside `Microsoft.Extensions.Configuration` values.

## Installation

Install the core package:

```shell
dotnet add package Invex.Rez
```

For `Microsoft.Extensions.Configuration` integration, install the additional package:

```shell
dotnet add package Invex.Rez.Configuration
```

Both packages target `net10.0` and `netstandard2.0`. On `netstandard2.0`,
`Invex.Rez` depends on `System.Buffers` and `System.Memory`. `Invex.Rez.Configuration` depends on
`Microsoft.Extensions.Configuration.Abstractions` and
`Microsoft.Extensions.DependencyInjection.Abstractions`.

## Quick start

```csharp
using Invex.Rez.Abstraction;
using Invex.Rez.Implementation;

var resolver = new Resolver()
    .AddSource(new ResolverSource(
        new Dictionary<string, string>
        {
            ["name"] = "World"
        },
        new Dictionary<string, Func<FunctionCall, string>>
        {
            ["shout"] = call => (call.Args ?? string.Empty).ToUpperInvariant() + "!"
        }));

var output = resolver.Resolve("{shout(hello)}, {name}! {unknown}");

// output: "HELLO!, World! {unknown}"
```

## Usage

### Variables and functions

Sources provide variables, functions, or both. Functions receive a `FunctionCall` whose `Args`
property contains all text between the parentheses; Rez does not split or trim arguments, so
parsing is owned by the function. Function delegates must return a non-null string; a source
returns `null` only when it cannot resolve a name and wants the next source to be queried.

```csharp
var resolver = new Resolver()
    .AddSource(new ResolverSource(new Dictionary<string, Func<FunctionCall, string>>
    {
        ["shout"] = call => (call.Args ?? string.Empty).ToUpperInvariant() + "!"
    }));

var output = resolver.Resolve("{shout(hello)}");

// output: "HELLO!"
```

See the [Syntax Guide](https://github.com/Invex-Games/rez/blob/main/docs/syntax.md) for the
complete template language.

### Source ordering and mutable values

`ResolverSource` is immutable and initialized with known values; `ResolverStore` is mutable and
replaces an existing entry when the same name is added. Sources are queried in registration order
for every placeholder and the first non-null result wins, so register overrides before defaults:

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
source types are not synchronized; configure them before concurrent use, or provide
synchronization when mutating them. See
[Getting Started](https://github.com/Invex-Games/rez/blob/main/docs/getting-started.md).

### Configuration integration

`Invex.Rez.Configuration` provides two related features:

1. `ConfigResolverSource` exposes `IConfigurationRoot` values as Rez variables, including nested
   keys such as `{Logging:Level}`.
2. `IResolvableConfig` wraps configuration and resolves templates when values are read. The
   `AddResolvableConfiguration()` extension registers it in dependency injection. Values written
   through it are stored unmodified; resolution occurs on read.

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var resolver = new Resolver()
    .AddSource(new ConfigResolverSource(config));

var output = resolver.Resolve("The {ServiceName} service is ready.");
```

See [Configuration Integration](https://github.com/Invex-Games/rez/blob/main/docs/configuration.md)
for dependency-injection setup.

### Custom sources

Implement `IResolverSource` to pull values from anywhere: environment variables, a database, or
computed values. Return `null` for names the source does not know so the next source is queried.
See the [Developer Guide](https://github.com/Invex-Games/rez/blob/main/docs/developer-guide.md).

## Documentation

Full documentation: [https://invex-games.github.io/rez/](https://invex-games.github.io/rez/)

| Guide | Contents |
| --- | --- |
| [Introduction](https://github.com/Invex-Games/rez/blob/main/docs/introduction.md) | What Rez is, when to use it, and its key concepts |
| [Getting Started](https://github.com/Invex-Games/rez/blob/main/docs/getting-started.md) | Install and resolve your first template |
| [Syntax Guide](https://github.com/Invex-Games/rez/blob/main/docs/syntax.md) | The complete template language reference |
| [Examples](https://github.com/Invex-Games/rez/blob/main/docs/examples.md) | Worked examples from simple to advanced |
| [Configuration Integration](https://github.com/Invex-Games/rez/blob/main/docs/configuration.md) | Resolving templates in app configuration |
| [Developer Guide](https://github.com/Invex-Games/rez/blob/main/docs/developer-guide.md) | Abstractions, custom sources, and how resolution works |
| [API Reference](https://invex-games.github.io/rez/api/index.html) | Generated reference for every public type |

## Development

The repository requires the .NET 10 SDK selected by `global.json`:

```shell
dotnet build Invex.Rez.slnx
dotnet test Invex.Rez.slnx
```

Build and preview the documentation site with the Atom build:

```shell
atom BuildDocs
atom ServeDocs
```

See [AGENTS.md](https://github.com/Invex-Games/rez/blob/main/AGENTS.md) for repository structure,
code conventions, generated workflow rules, snapshot testing, and the complete contributor
checklist.

## Where did the name come from?

The name Rez is a play on the word "res" (short for "resolution"), which is the process of
resolving a template. The name was chosen as it is short and memorable and is not too similar to
other names in the .NET ecosystem.

## License

Rez is licensed under the MIT License. See
[LICENSE.txt](https://github.com/Invex-Games/rez/blob/main/LICENSE.txt).
