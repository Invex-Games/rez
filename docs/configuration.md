# Configuration Integration

The `Invex.Rez.Configuration` package integrates Rez with
`Microsoft.Extensions.Configuration`, enabling two complementary scenarios:

1. **Configuration as a source** — use configuration values as Rez variables
   (`ConfigResolverSource`)
2. **Templates inside configuration** — write Rez templates *in* your configuration files and
   have them resolved transparently when values are read (`IResolvableConfig`)

## Installation

```shell
dotnet add package Invex.Rez.Configuration
```

## Configuration as a variable source

`ConfigResolverSource` is an `IResolverSource` that resolves variables from an
`IConfigurationRoot`:

> appsettings.json

```json
{
  "ServiceName": "orders-api",
  "Logging": {
    "Level": "Information"
  }
}
```

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var resolver = new Resolver()
    .AddSource(new ConfigResolverSource(config));

resolver.Resolve("Starting {ServiceName} with log level {Logging:Level}");

// "Starting orders-api with log level Information"
```

Nested configuration keys are addressed using the standard colon-delimited form
(e.g., `{Logging:Level}`), exactly as with `IConfiguration` indexers. This source provides
variables only — it does not supply functions.

## Templates inside configuration values

`IResolvableConfig` wraps an `IConfigurationRoot` so that any value read from it is resolved
with an `IResolver` first. This lets configuration values reference each other (or anything else
your resolver's sources provide):

> appsettings.json

```json
{
  "BaseUrl": "https://api.example.com",
  "Endpoints": {
    "Orders": "{BaseUrl}/orders",
    "Customers": "{BaseUrl}/customers"
  }
}
```

### Registration with dependency injection

Use the `AddResolvableConfiguration()` extension method. It registers an `IResolvableConfig`
singleton that requires `IConfigurationRoot` and `IResolver` to be registered as well:

```csharp
var config = (IConfigurationRoot)new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var resolver = new Resolver()
    .AddSource(new ConfigResolverSource(config));

var services = new ServiceCollection()
    .AddSingleton<IConfigurationRoot>(config)
    .AddSingleton<IResolver>(resolver)
    .AddResolvableConfiguration()
    .BuildServiceProvider();

var resolvableConfig = services.GetRequiredService<IResolvableConfig>();

var ordersUrl = resolvableConfig["Endpoints:Orders"];

// ordersUrl: "https://api.example.com/orders"
```

### Behavior

- Values read via the indexer, `GetSection()`, `GetChildren()`, and section `Value` properties
  are resolved automatically.
- Child sections are wrapped too, so resolution applies at any depth.
- **Writes pass through unmodified** — assigning a value stores the raw text, templates intact.
- `IResolvableConfig` also exposes the `Resolver` used and a convenience
  `Resolve(string?)` method for resolving arbitrary text with the same resolver.
- Since `IResolvableConfig` implements `IConfigurationRoot`, it can be handed to any API that
  accepts `IConfiguration`/`IConfigurationRoot`, transparently adding template resolution.

### Escaping in JSON

Remember that backslashes must be escaped in JSON, so a literal brace in a configuration value is
written as:

```json
{
  "myTemplate": "\\{escapedVariable\\}"
}
```

## Combining both scenarios

A common setup uses the configuration itself as the variable source for resolving the
configuration's own values — this is exactly what the registration example above does:
`ConfigResolverSource` feeds variables from `appsettings.json` into the resolver, and
`IResolvableConfig` applies that resolver to every value read, so configuration values can
reference each other freely.

