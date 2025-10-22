# Argx

A modern command-line argument parsing library for dotnet.

The library aims to provide an easier way to write user-friendly command-line applications.  
The application should define the arguments it requires and the library will figure out how to parse them. It will automatically generate help and usage messages, as well as errors when the user input is not valid.

## ArgumentParser

The argument parsing is done via `ArgumentParser`, which provides functionality for defining, parsing, and managing command-line arguments.

``` csharp
var parser = new ArgumentParser(
    app: "app_name",
    description: "What the application does",
    epilogue: "Text at the bottom of the help message");
```

### Adding arguments

The `Add` method is used to add individual argument specifications to the parser. It supports positional arguments, options and flags.

``` csharp
parser.Add("file");                 // positional argument
parser.Add<int>("--level");         // option of type int
parser.Add<bool>("--debug", ["-d"], // flag
    usage: "enable debug mode",
    action: ArgumentActions.StoreTrue);
```

However, it’s recommended to use the dedicated methods for each argument type: `AddArgument`, `AddOption`, and `AddFlag`. These methods accept only the parameters relevant to their specific type.  
All the add have methods a generic overload for specifying the argument’s value type.

### AddArgument

Positional arguments are identified by their position in the command line, rather than by a flag or option name, their meaning depends on where they appear in the command. This means the positional arguments will be parsed in the ordered they were added.

Use `AddArgument` to add a positional argument

``` csharp
var parser = new ArgumentParser();
parser.AddArgument<int>("x");
parser.AddArgument<int>("y");

var argx = parser.Parse(["2", "3"]);
var x = argx.GetValue<int>("x");
var y = argx.GetValue<int>("y");

Console.WriteLine(x + y); // Outputs: 5
```

### AddOption

Options are identified by name. They must start with `-` or `--`.

``` csharp
var parser = new ArgumentParser();
parser.AddArgument("echo", usage: "The string to be echoed back");
parser.AddOption<string>("--prefix", ["-p"], usage: "A prefix to add before the echoed string");
parser.AddOption<int>("--count", ["-c"], usage: "Number of times to echo the string");

var argx = parser.Parse(args);
var echo = argx.GetRequired<string>("echo");
var prefix = argx.GetValue<string>("prefix") ?? string.Empty;
if (!argx.TryGetValue<int>("count", out var count))
    count = 1;

for (int i = 0; i < count; i++)
{
    Console.WriteLine($"{prefix}{echo}");
}
```

``` bash
$ dotnet run -- world --prefix "hello " -c 3
hello world
hello world
hello world
```
`NOTE`: -- is used to separate the application arguments from the dotnet run arguments.

`NOTE`: When storing the value, dashes are automatically removed. So if you add an argument like --count, you’ll access it using count. To override this behavior, use the dest parameter.

### AddFlag

A flag is an option that represents a boolean value, it’s either present (true) or absent (false). They are options, so they must start with `-`, or `--`.

``` csharp
parser.AddFlag("--verbose", ["-v"], "Enable verbose output");
var argx = parser.Parse(["-v"]);
Console.WriteLine(argx.GetValue<bool>("verbose")); // Outputs: True
```

`NOTE`: When storing the value, dashes are automatically removed. So if you add an argument like --count, you’ll access it using count. To override this behavior, use the dest parameter.

### Actions

Actions define what happens when an argument is encountered during parsing.  
Built-in actions are:

**`store`**: Stores the argument’s value(s). This is the default action when no action is explicitly specified.

``` csharp
parser.Add<string>("foo");
var argx = parser.Parse(["bar"]);
Console.WriteLine(argx.GetValue<string>("foo")); // output: "bar"
```

**`store_const`**: Stores the predefined `constValue` when the **option** is encountered.

``` csharp
parser.Add<string>("--foo", action: ArgumentActions.StoreConst, constValue: "bar");
var argx = parser.Parse(["--foo"]);
Console.WriteLine(argx.GetValue<string>("foo")); // Outputs: bar
```

**`store_true`**: Stores `true` when the **option** is encountered.

``` csharp
parser.Add<string>("--foo", action: ArgumentActions.StoreTrue);
var argx = parser.Parse(["--foo"]);
Console.WriteLine(argx.GetValue<bool>("foo")); // Outputs: True
```

**`store_false`**: Stores `false` when the **option** is encountered.

``` csharp
parser.Add<string>("--foo", action: ArgumentActions.StoreFalse);
var argx = parser.Parse(["--foo"]);
Console.WriteLine(argx.GetValue<bool?>("foo")); // Outputs: False
```
`NOTE`: nullable is used because GetValue<T> returns default(T) if the key doesn't exist, which is false for bool. Using TryGetValue would also work.

**`count`**: Stores the number of times the argument is encountered.

``` csharp
parser.Add<int>("-v",
    action: ArgumentActions.Count,
    usage: "Increase verbosity level",
    dest: "verbosity");
var argx = parser.Parse(["-vvv"]);
Console.WriteLine(argx.GetValue<int>("verbosity")); // Outputs: 3
```
**`append`**: Appends each value to a list. Useful when an option can appear multiple times.

``` csharp
parser.Add<int[]>("--foo", action: ArgumentActions.Append, arity: Arity.AtLeastOne);
var argx = parser.Parse(["--foo", "0", "--foo", "1", "2", "--bar", "10", "--foo", "3"]);
Console.WriteLine(string.Join(", ", argx.GetValue<int[]>("foo"))); // Outputs: 0, 1, 2
```
**NOTE**: `Arity.AtLeastOne` reads all the following values until another option is encountered or end is reached. This means we can append both individual values and collections of values. If arity was 1, then it would append only the following value. `Arity.Any` works in a similar way but it needs a `constValue` when no value is provided. See [arity](#arity) for more details.

#### **Custom actions**

To register a custom action, use the `ArgumentParser.AddAction` method. It takes an action name and an `ArgumentAction` instance. Once added, you can use the action by specifying its name in the action parameter of any add method.

`ArgumentAction` is the base type for all actions. Every action implementation derives from it and must define two methods:

**Validate**

Called when a new argument specification is added to the parser. It ensures that the action is valid and can be performed. Each action has its own validation rules and throws an exception if they are not met.
This helps catch configuration errors at startup, rather than at runtime during parsing.

**Execute**

Called when the argument is encountered during parsing. This method performs the action’s logic, such as storing values, it receives the argument definition, the token that triggered it, any associated value tokens, and the argument repository where parsed results are stored.  
Each action overrides this method to implement its specific behavior.  
The base implementation validates that the invocation is not null or empty.

### Arity

Defines the arity (number of values) an argument can accept. This can be either a fixed number or a keyword.

#### **Keywords**

**`? (Arity.Optional)`**: An optional value, one argument will be consumed from the command line if possible, and produced as a single item. This should be used along `constValue`, which will be used if no value is provided.

``` csharp
parser.Add("--foo", arity: Arity.Optional, constValue: "0");
parser.Add("--bar", arity: Arity.Optional, constValue: "0");
var result = parser.Parse(["--foo", "--bar", "baz"]);
Console.WriteLine(result.GetValue("foo")); // Outputs: 0
Console.WriteLine(result.GetValue("bar")); // Outputs: baz
```

**`* (Any)`**:  Any number of values. The argument can be provided with zero or more values. The argument type has to be a collection type. This will consume all the following values until another option is encountered or reaches the end of the command line.

``` csharp
parser.Add<int[]>("--foo", arity: Arity.Any, constValue: Array.Empty<int>());
parser.Add<int[]>("--bar", arity: Arity.Any, constValue: Array.Empty<int>());
var result = parser.Parse(["--foo", "--bar", "0", "1", "2"]);
Console.WriteLine($"foo: {string.Join(", ", result.GetValue<int[]>("foo"))}"); // Outputs: foo: 
Console.WriteLine($"bar: {string.Join(", ", result.GetValue<int[]>("bar"))}"); // Outputs: bar: 0, 1, 2, 3
```

**`+ (AtLeastOne)`**: At least one value, same as `any`, but throws if no value is provided.

``` csharp
parser.Add<int[]>("--foo", action: ArgumentActions.Append, arity: Arity.AtLeastOne);
var argx = parser.Parse(["--foo", "0", "--foo", "1", "2", "--bar", "10", "--foo", "3"]);
Console.WriteLine(string.Join(", ", argx.GetValue<int[]>("foo"))); // Outputs: 0, 1, 2
```

### Add parameters

- **T**: The expected type of the value.
- **name**: The primary name of the option. Positional arguments should be specified without prefixes, while optional arguments should start with dashes.
- **alias**: Optional alternative names for the option. Usually used for short options. Use null for no alias. Must be null for positional arguments.
- **usage**: A short description of the argument’s purpose, displayed in help output.
- **dest**: The key used to retrieve the value from the parsed result. If null, the parser infers it from the name.
- **metavar**: The placeholder name displayed in help messages for the argument’s value. Displayed only for arguments that expect values.
- **constValue**: A constant value assigned when the argument is used without an explicit value. Relevant mainly for certain actions.
- **action**: The action to perform when the argument is encountered, see [actions](#actions).
- **arity**: Specifies how many values the argument expects, use null to reply on defaults inferred from the action. see [arity](#arity) for more details.
- **choices**: A set of allowed values for the argument. If specified, the argument's value must be one of these choices.