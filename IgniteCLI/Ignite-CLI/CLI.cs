using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IgniteCLI
{
    class InputCommand
    {
        public string Name;
        public Dictionary<string, string> Arguments;
    }

    public class CLI
    {
        #region Convenience Functions
        public static string String(Dictionary<string, string> args, string key) => args.ContainsKey(key.ToLower()) ? args[key.ToLower()] : null;
        public static int? Int(Dictionary<string, string> args, string key) => String(args, key) != null ? Convert.ToInt32(String(args, key)) : (int?)null;
        public static bool Bool(Dictionary<string, string> args, string key) => args.ContainsKey(key.ToLower()) ? args[key.ToLower()].ToLower() == "true" : false;
        public static T Enum<T>(Dictionary<string, string> args, string key) => String(args, key).ToEnum<T>();
        #endregion

        private static CommandList Commands;
        private static readonly CommandList DefaultCommands = new CommandList
        {
            new Command
            {
                Name = "colors",
                Description = "Displays examples for all available console colors",
                Function = args =>
                {
                    var colors = System.Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>();
                    foreach (var c in colors)
                        CLI.Out(c.ToString(), c);
                    foreach (var c in colors)
                        CLI.Out(c.ToString(), ConsoleColor.Gray, c);
                }
            },
            new Command
            {
                Name = "help",
                Description = "Shows this list of commands",
                Function = args =>
                {
                    if(args.Count > 0)
                    {
                        var cmd = Commands.FirstOrDefault(x => x.Name.ToLower() == args.Keys.First());
                        if(cmd != null)
                        {
                            CLI.Help(cmd);
                            return;
                        }
                    }

                    CLI.Help();
                }
            },
        };

        public static void Start(CommandList commands)
        {
            Commands = commands;
            foreach (var cmd in DefaultCommands)
            {
                if (!Commands.Any(x => x.Name == cmd.Name))
                    Commands.Insert(0, cmd);
            }

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Write("> ");

            var input = Console.ReadLine();
            while (input != "exit")
            {
                if (input.Length != 0)
                {
                    Run(input);
                    CLI.Out();
                }

                Console.Write("> ");
                input = Console.ReadLine();
            }
        }

        private static InputCommand ParseInput(string input)
        {
            string cmd = "help";
            var cmdArgs = new Dictionary<string, string>();
            if (input.Contains(" "))
            {
                try
                {
                    cmd = input.Substring(0, input.IndexOf(" "));
                    if (cmd.ToLower() == "help")
                    {
                        cmdArgs.Add(input.Substring(5), "");
                    }
                    else
                    {
                        var tokens = input.Substring(input.IndexOf(" ") + 2).Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var t in tokens)
                        {
                            if (t.Contains(' '))
                                cmdArgs.Add(t.Substring(0, t.IndexOf(' ')).ToLower(), t.Substring(t.IndexOf(' ') + 1));
                            else
                                cmdArgs.Add(t, "true");
                        }
                    }
                }
                catch { } //TODO: not this
            }
            else
            {
                cmd = input;
            }

            return new InputCommand
            {
                Name = cmd,
                Arguments = cmdArgs
            };
        }

        public static void Break() => Out("-----------------------------------------------------", ConsoleColor.Green);
        public static void Out(string s = "") => Console.WriteLine(s);
        public static void Out(string s, ConsoleColor fore, ConsoleColor back = ConsoleColor.Black)
        {
            Console.ForegroundColor = fore;
            Console.BackgroundColor = back;
            Out(s);
            Console.ResetColor();
        }

        public static void Help()
        {
            Out("HELP: cmd -arg [value] {-optionalArg [optional value]} {-optionalBool}");
            Break();

            foreach (var cmd in Commands)
            {
                Help(cmd);
                Out();
            }

            Break();
        }

        private static void Help(Command cmd)
        {
            Out($"{cmd.Name} {cmd.Format()}", ConsoleColor.Green);
            Out($"# {cmd.Description}", ConsoleColor.Cyan);
            foreach (var arg in cmd.Args)
            {
                Out($"| {arg.Tag} : {arg.Description}", ConsoleColor.DarkCyan);
            }
        }

        private static void Run(InputCommand cmd)
        {
            Command exec = Commands[cmd.Name.ToLower()] ?? Commands["help"];
            if (!exec.RequiredArgs.Select(x => x.Tag).All(x => cmd.Arguments.ContainsKey(x)))
                exec = Commands["help"];

            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                exec.Function.Invoke(cmd.Arguments);
                sw.Stop();
                CLI.Out(sw.ElapsedMilliseconds + "ms");
            }
            catch (Exception e)
            {
                sw.Stop();
                CLI.Out(e.Message, ConsoleColor.Red);
            }
        }

        public static void Run(string input) => CLI.Run(ParseInput(input));

        public static void Run(string command, params string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            foreach (string s in args)
            {
                var ss = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                arguments.Add(ss[0], ss[1]);
            }
            Run(new InputCommand { Name = command, Arguments = arguments });
        }

        public static void Run(params string[] input)
        {
            string command = input[0];
            string[] realArgs = new string[input.Length - 1];
            Array.Copy(input, 1, realArgs, 0, realArgs.Length);
            CLI.Run(command, realArgs);
        }
    }
}