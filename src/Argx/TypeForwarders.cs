using System.Runtime.CompilerServices;

using Argx;
using Argx.Actions;
using Argx.Binding;
using Argx.Errors;
using Argx.Help;
using Argx.Parsing;
using Argx.Subcommands;

// Preserve references from consumers compiled before the core types moved to Argx.AOT.
[assembly: TypeForwardedTo(typeof(AliasSet))]
[assembly: TypeForwardedTo(typeof(Argument))]
[assembly: TypeForwardedTo(typeof(Arguments))]
[assembly: TypeForwardedTo(typeof(Arity))]
[assembly: TypeForwardedTo(typeof(CommandLineApplication))]
[assembly: TypeForwardedTo(typeof(IArguments))]
[assembly: TypeForwardedTo(typeof(ICommandLineApplication))]
[assembly: TypeForwardedTo(typeof(ArgumentAction))]
[assembly: TypeForwardedTo(typeof(ArgumentActions))]
[assembly: TypeForwardedTo(typeof(ArgumentConversionDefaults))]
[assembly: TypeForwardedTo(typeof(CollectionFactoryRegistry))]
[assembly: TypeForwardedTo(typeof(NumberStylesOptions))]
[assembly: TypeForwardedTo(typeof(ArgumentValueException))]
[assembly: TypeForwardedTo(typeof(NoSubcommandException))]
[assembly: TypeForwardedTo(typeof(UnknownSubcommandException))]
[assembly: TypeForwardedTo(typeof(HelpConfiguration))]
[assembly: TypeForwardedTo(typeof(ArgumentParser))]
[assembly: TypeForwardedTo(typeof(ArgumentParserConfiguration))]
[assembly: TypeForwardedTo(typeof(ArgumentStore))]
[assembly: TypeForwardedTo(typeof(IArgumentParser))]
[assembly: TypeForwardedTo(typeof(IArgumentStore))]
[assembly: TypeForwardedTo(typeof(Token))]
[assembly: TypeForwardedTo(typeof(TokenType))]
[assembly: TypeForwardedTo(typeof(AsyncSubcommandDelegate))]
[assembly: TypeForwardedTo(typeof(Subcommand))]
[assembly: TypeForwardedTo(typeof(SubcommandDelegate))]
