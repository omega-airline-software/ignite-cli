![logo](https://raw.githubusercontent.com/omega-airline-software/ignite-cli/master/images/Icon.png)
# Ignite CLI

[Ignite](https://www.nuget.org/packages/IgniteCLI) is a library for creating CLI applications in .NET Core

## Overview

Ignite helps you easily and quickly turn a Console App into a fully-fledged Command Line Interface. When Ignite is given a list of Commands, setup is complete. It will then take over the console and parse user input automatically then trigger the given functions with the appropriate arguments.

## Getting Started

1.  Create a new .NET Console App (.NET Core 2.0) project
2.  Follow Installation
3.  Declare and define a `CommandList`
4.  Run `CLI.Start(MyCommandList)` in the `static void Main(string[] args)` method that's provided with Console Apps

## Installation

To install [Ignite](https://www.nuget.org/packages/IgniteCLI) simply find the package in the Nuget package browser or run

```
Install-Package IgniteCLI
```

**NOTE:** .NET Core 2.0 is required in order to use Ignite.

# Usage

## API

-   `CLI.Start(CommandList commands)`
    -   This method is what initializes Ignite with the given `CommandList`

#### Printing

Ignite provides multiple methods for output, including the ability to customize color

-   `CLI.Help()`
    -   Help prints out the `CommandList` in an easy-to-read format
-   `CLI.Break()`
    -   Outputs a line of hypens to a new line in the Console
-   `CLI.Out(string s = "")`
    -   Outputs `s` to a new line in the Console
-   `CLI.Out(string s, ConsoleColor fore, ConsoleColor back = ConsoleColor.Black)`
    -   Outputs `s` to a new line in the Console with the given foreground and background color

#### Programmability

Ignite lets you simulate user input for programmatic command execution

-   `CLI.Run(string command, params string[] args)`
    -   Runs the `command` with the provided `args` as if they were typed into the Console by a user
    -   Example: `CLI.Run("add", "a 10", "b 5")`
    -   Example: `CLI.Run("add", new string[2] {"a 10", "b 5"})`
-   `CLI.Run(params string[] input)`
    -   Extracts the first element in the array and calls the above `CLI.Run()` passing that in as the `command` and the rest as the `args`
    -   Example: `CLI.Run("add", "a 10", "b 5")`
    -   Example: `CLI.Run(new string[3] {"add", "a 10", "b 5"})`
-   `CLI.Run(string input)`
    -   Parses the string just like raw user input
    -   Example: `CLI.Run("add -a 10 -b 5")`

#### Parsing Arguments

During command functions, Ignite provides quick methods to automatically parse and convert arguments

-   `CLI.String(Dictionary<string, string> d, string key)`
    -   Parses the value from the `Dictionary<string, string>` as a String
-   `CLI.Int(Dictionary<string, string> d, string key)`
    -   Parses the value from the `Dictionary<string, string>` as a Int
-   `CLI.Bool(Dictionary<string, string> d, string key)`
    -   Parses the value from the `Dictionary<string, string>` as a Bool
    -   "true" and "false" are evaluated appropriately
    -   When no value is given, it is evaluated as true

# Commands

## Structure

```cs
public static CommandList Commands = new CommandList
{
    new Command
    {
        Name = "mycommand",
        Description = "Executes My Command with the given arguments",
        Args = new CommandArgs
        {
            new CommandArg
            {
                Tag = "string-in",
                InputFormat = "Informs user of the value to provide for 'string-in'",
                Description = "(str) This is my first CommandArg named 'string-in'",
                //Required = true by default
            },
        },
        Function = args =>
        {
            string input = CLI.String(args, "string-in");
            if(String.IsNullOrEmpty(input)) CLI.Help(); //No input given, show help
            else
            {
                CLI.Out(MyCommand(input));
            }
        }
    },
}
```

## Example

```cs
new Command
{
    Name = "add",
    Description = "Add two numbers together",
    Args = new CommandArgs
    {
        new CommandArg
        {
            Tag = "a",
            InputFormat = "first number",
            Description = "(int) The first number in the equation",
        },
        new CommandArg
        {
            Tag = "b",
            InputFormat = "second number",
            Description = "(int) The second number in the equation",
        }
    },
    Function = args =>
    {
        int a = CLI.Int(args, "a");
        int b = CLI.Int(args, "b");
        CLI.Out($"{a} + {b} = {a + b}", ConsoleColor.Green);
    }
},
```

### Expected Output

![Output](https://i.imgur.com/NwEuNyw.png?1)

### Generated Help

![Help](https://i.imgur.com/z7SJB0p.png?1)

---

# Sample Program

This is an example of a simple CLI made with Ignite.

```cs
using System;
using IgniteCLI;

namespace IgniteExample
{
    class Program
    {
        static CommandList commands = new CommandList
        {
            new Command
            {
                Name = "hello",
                Description = "Says hello",
                Args = new CommandArgs
                {
                    new CommandArg
                    {
                        Tag = "name",
                        InputFormat = "user name",
                        Description = "(string) The name used when saying hello",
                        Required = false,
                    }
                },
                Function = args =>
                {
                    string name = CLI.String(args, "name");
                    CLI.Out($"Hello, {(String.IsNullOrEmpty(name) ? "World" : name)}!");
                }
            },
        };
        static void Main(string[] args)
        {
            CLI.Start(commands);
        }
    }
}
```
