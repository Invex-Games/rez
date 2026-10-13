# Examples

This page walks through complete examples, from a simple greeting to a multi-source application.
Use it to see how variables, functions, nesting and configuration combine in realistic templates.
The configuration-based examples need the `Invex.Rez.Configuration` package and
`Microsoft.Extensions.Configuration.Json` for `ConfigurationBuilder.AddJsonFile`.

## Simple example: Greeting

Template:

```text
Hello, {name}! Welcome to our platform.
```

Variables:

```json
{
  "name": "John"
}
```

Resolved text:

```text
Hello, John! Welcome to our platform.
```

In code:

```csharp
var resolver = new Resolver();

resolver.AddSource(new ResolverSource(new Dictionary<string, string>
{
    { "name", "John" }
}));

var output = resolver.Resolve("Hello, {name}! Welcome to our platform.");

// output: "Hello, John! Welcome to our platform."
```

## Example with a function: Temperature conversion

Template:

```text
The temperature is {temperature} degrees Fahrenheit ({fahrenheitToCelsius({temperature})} degrees Celsius).
```

Variables:

```json
{
  "temperature": "68"
}
```

Functions:

```text
fahrenheitToCelsius(fahrenheit) => (fahrenheit - 32) * 5 / 9
```

Resolved text:

```text
The temperature is 68 degrees Fahrenheit (20 degrees Celsius).
```

In code:

```csharp
var resolver = new Resolver();

resolver.AddSource(new ResolverSource(
    new Dictionary<string, string>
    {
        { "temperature", "68" }
    },
    new Dictionary<string, Func<FunctionCall, string>>
    {
        {
            "fahrenheitToCelsius", call =>
            {
                var fahrenheit = double.Parse(call.Args ?? "0");

                return ((fahrenheit - 32) * 5 / 9).ToString("0.#");
            }
        }
    }));

var output = resolver.Resolve(
    "The temperature is {temperature} degrees Fahrenheit " +
    "({fahrenheitToCelsius({temperature})} degrees Celsius).");
```

Note how `{temperature}` is nested inside the function call — the inner variable is resolved
first, so the function receives `"68"` as its argument text.

## Example with nested variables and functions: Order details

Template:

```text
Dear {customer:name},

Thank you for your order of {product:name} ({product:sku}). Your order number is {order:number}.

The total cost of your order is {calculateTotal({order:price},{order:tax})}.

Best regards,
{company:name}
```

Variables (e.g., loaded from configuration — note the colon-delimited keys for nested sections):

```json
{
  "customer": {
    "name": "Jane"
  },
  "product": {
    "name": "Wireless Headphones",
    "sku": "WH-123"
  },
  "order": {
    "number": "ORD-456",
    "price": "100",
    "tax": "10"
  },
  "company": {
    "name": "Electronics Store"
  }
}
```

Functions:

```text
calculateTotal(price,tax) => price + tax
```

Resolved text:

```text
Dear Jane,

Thank you for your order of Wireless Headphones (WH-123). Your order number is ORD-456.

The total cost of your order is $110.

Best regards,
Electronics Store
```

In code:

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("order.json")
    .Build();

var resolver = new Resolver();

// Variables come from configuration; nested keys use the colon-delimited form, e.g. {customer:name}
resolver.AddSource(new ConfigResolverSource(config));

// Functions come from a separate source
resolver.AddSource(new ResolverSource(new Dictionary<string, Func<FunctionCall, string>>
{
    {
        "calculateTotal", call =>
        {
            var parts = (call.Args ?? string.Empty).Split(',');
            var total = decimal.Parse(parts[0]) + decimal.Parse(parts[1]);

            return $"${total}";
        }
    }
}));

var output = resolver.Resolve(template);
```

## Sample application: User input + configuration

Let's say we have a console application that reads a configuration from a JSON file and uses that
configuration to populate variables. The text to resolve will come from user input.

> appsettings.json

```json
{
  "animal1": "fox",
  "animal2": "dog",
  "color": "brown",
  "description": "lazy"
}
```

> App code

```csharp
// Read the configuration from the JSON file
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Create a Resolver instance
var resolver = new Resolver();

// Add the config as a source for variable resolution
resolver.AddSource(new ConfigResolverSource(config));

// Get user input
Console.WriteLine("Enter a sentence:");
var input = Console.ReadLine();

// Resolve the text from the template
var output = resolver.Resolve(input);

// Output the resolved text
Console.WriteLine(output);
```

If we run the application and enter the following text:

```text
The quick {color} {animal1} jumped over the {description} {animal2}.
```

The output will be:

```text
The quick brown fox jumped over the lazy dog.
```

## See also

- [Syntax Guide](syntax.md): the complete template language reference
- [Configuration Integration](configuration.md): using configuration as a source, or templates inside configuration
- [Developer Guide](developer-guide.md): writing custom sources
- <xref:Invex.Rez.Implementation.ResolverSource> and <xref:Invex.Rez.Configuration.ConfigResolverSource>
