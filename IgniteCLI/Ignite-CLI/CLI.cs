using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
                        var cmd = Commands[args.Keys.First()];
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

        #region Output
        public static void Break() => CLI.Out("-----------------------------------------------------", ConsoleColor.Green);
        public static void Out(string s = "") => Console.WriteLine(s);
        public static void Out(string s, ConsoleColor fore, ConsoleColor back = ConsoleColor.Black)
        {
            Console.ForegroundColor = fore;
            Console.BackgroundColor = back;
            CLI.Out(s);
            Console.ResetColor();
        }
        #endregion

        #region Help
        public static void Help()
        {
            CLI.Out("HELP: cmd -arg [value] {-optionalArg [optional value]} {-optionalBool}");
            Break();

            foreach (var cmd in Commands)
            {
                Help(cmd);
                CLI.Out();
            }

            Break();
        }

        private static void Help(Command cmd)
        {
            CLI.Out($"{cmd.Name} {cmd.Format()}", ConsoleColor.Green);
            CLI.Out($"# {cmd.Description}", ConsoleColor.Cyan);
            foreach (var arg in cmd.Args)
            {
                CLI.Out($"| {arg.Tag} : {arg.Description}", ConsoleColor.DarkCyan);
            }
        }
        #endregion

        #region Run
        /// <summary>
        /// fuzzy search
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Command MostSimilarCommand(string name) => Commands.OrderBy(x => name.ToLower().DistanceFrom(x.Name.ToLower())).FirstOrDefault();

        private static void Run(InputCommand cmd)
        {
            Command exec = Commands[cmd.Name.ToLower()];
            if (exec == null)
            {
                var suggestedCommand = MostSimilarCommand(cmd.Name);
                if (suggestedCommand != null)
                {
                    Console.Write("Did you mean ");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(suggestedCommand.Name);
                    if (cmd.Arguments.Count > 0) Console.Write(" ");

                    StringBuilder args = new StringBuilder();
                    foreach (var a in cmd.Arguments)
                        args.Append($"-{a.Key} {a.Value} ");
                    if (cmd.Arguments.Count > 0) args.Remove(args.Length - 1, 1);

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write(args);

                    Console.ResetColor();
                    Console.WriteLine("? [Y/n]");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("> ");
                    exec = Console.ReadLine() == "Y" ? suggestedCommand : Commands["help"];
                    Console.ResetColor();
                }
                else exec = Commands["help"];
            }

            var missingRequiredArgs = exec.RequiredArgs.Select(x => x.Tag).Where(x => cmd.Arguments.ContainsKey(x)).ToList();
            if (missingRequiredArgs.Count > 0)
            {


                exec = Commands["help"];
            }

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

        /// <summary>
        /// Parse raw input and execute it
        /// </summary>
        /// <param name="input"></param>
        public static void Run(string input) => CLI.Run(ParseInput(input));

        /// <summary>
        /// Execute a command with arguments. Example: CLI.Run("mycmd", "arg1 val1", "arg2 val2"); or CLI.Run("mycmd", new string[] { "arg1 val1", "arg2 val2" });
        /// This is useful for dynamic command execution
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
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

        /// <summary>
        /// Execute raw input separated into arguments. Example: CLI.Run("mycmd", "arg1 val1", "arg2 val2"); or CLI.Run(new string[] { "mycmd", "arg1 val1", "arg2 val2" });
        /// This is useful for different patterns of dynamic command execution than Run(string command, params string[] args)
        /// </summary>
        /// <param name="input"></param>
        public static void Run(params string[] input)
        {
            string command = input[0];
            string[] realArgs = new string[input.Length - 1];
            Array.Copy(input, 1, realArgs, 0, realArgs.Length);
            CLI.Run(command, realArgs);
        }
        #endregion
    }
}