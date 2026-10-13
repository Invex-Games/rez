# Rez

Rez is a small .NET library for resolving terse, nestable template text such as `{name}` and
`{function(args)}`. It is designed for application strings, configuration values, and other text
where a full template engine would be unnecessary.

## Start here

- [Introduction](docs/introduction.md): what Rez is, when to use it, and its key concepts
- [Getting Started](docs/getting-started.md): install the package and resolve your first template
- [Syntax Guide](docs/syntax.md): the complete template language reference
- [Configuration Integration](docs/configuration.md): resolving templates in app configuration
- [API Reference](api/index.md): generated reference for every public type

## Packages

| Package | What it does |
| --- | --- |
| [Invex.Rez](https://www.nuget.org/packages/Invex.Rez) | The core templating library: `Resolver`, `ResolverSource`, and `ResolverStore`. |
| [Invex.Rez.Configuration](https://www.nuget.org/packages/Invex.Rez.Configuration) | Integration with `Microsoft.Extensions.Configuration` and dependency injection. |
