using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IgniteCLI
{
    public class CLI
    {
        #region Convenience Functions
        public static string String(Dictionary<string, string> args, string key) => args.ContainsKey(key.ToLower()) ? args[key.ToLower()] : null;
        public static int? Int(Dictionary<string, string> args, string key) => String(args, key) != null ? Convert.ToInt32(String(args, key)) : (int?)null;
        public static bool Bool(Dictionary<string, string> args, string key) => args.ContainsKey(key.ToLower()) ? args[key.ToLower()].ToLower() == "true" : false;
        public static T Enum<T>(Dictionary<string, string> args, string key) => String(args, key).ToEnum<T>();
        #endregion

        public static IgniteOptions Options = new IgniteOptions();

        private static bool Stopped = false;
        private static CommandList Commands;
        private static CommandList DefaultCommands = new CommandList();

        private static string InputPrefix = "> ";

        public static void Start(CommandList commands)
        {
            Initialize(commands);
            
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            CLI.Out(InputPrefix);

            var input = Console.ReadLine();
            while (!Stopped)
            {
                if (input.Length != 0)
                {
                    Run(input);
                    CLI.Line();
                }

                Console.Write(InputPrefix);
                input = Console.ReadLine();
            }
        }

        private static void Initialize(CommandList commands)
        {
            Stopped = false;

            //core logic requires "help" command always existing, need to make this not true in order to provide an option to remove the help command
            //all DefaultCommands are overrideable by regular Commands still, though.
            DefaultCommands.Add(new Command
            {
                Name = "help",
                Description = "Shows this list of commands",
                Function = args =>
                {
                    if (args.Count > 0)
                    {
                        var input = args.Keys.First();
                        var cmd = Commands[input];
                        if (cmd == null)
                        {
                            var top3 = FuzzyCommandSearch(input).Take(3);
                            foreach (var c in top3)
                                Help(c);
                            return;
                        }
                        else
                        {
                            CLI.Help(cmd);
                            return;
                        }
                    }

                    CLI.Help();
                }
            });
            if (Options.EnableColorsCommand)
                DefaultCommands.Add(new Command
                {
                    Name = "colors",
                    Description = "Displays examples for all available console colors",
                    Function = args =>
                    {
                        var colors = System.Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>();
                        foreach (var c in colors)
                            CLI.Line(c.ToString(), c);
                        foreach (var c in colors)
                            CLI.Line(c.ToString(), ConsoleColor.Gray, c);
                    }
                });
            if (Options.EnableExitCommand)
                DefaultCommands.Add(new Command
                {
                    Name = "exit",
                    Function = args => { CLI.Stop(); }
                });

            Commands = commands;
            foreach (var cmd in DefaultCommands)
            {
                if (!Commands.Any(x => x.Name == cmd.Name))
                    Commands.Insert(0, cmd);
            }
        }

        public static void Stop() => Stopped = true;

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
        public static void Break() => Line("-----------------------------------------------------", ConsoleColor.Green);
        public static void Line(string s = "") => Console.WriteLine(s);
        public static void Line(string s, ConsoleColor fore, ConsoleColor back = ConsoleColor.Black)
        {
            Out(s, fore, back);
            Line();
        }
        public static void Out(string s = "") => Console.Write(s);
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
            CLI.Line("HELP: cmd -arg [value] {-optionalArg [optional value]} {-optionalBool}");
            Break();

            foreach (var cmd in Commands)
            {
                Help(cmd);
                CLI.Line();
            }

            Break();
        }

        private static void Help(Command cmd)
        {
            CLI.Line($"{cmd.Name} {cmd.Format()}", ConsoleColor.Green);
            CLI.Line($"# {cmd.Description}", ConsoleColor.Cyan);
            foreach (var arg in cmd.Args)
            {
                CLI.Line($"| {arg.Tag} : {arg.Description}", ConsoleColor.DarkCyan);
            }
        }
        #endregion

        /// <summary>
        /// A list of all Commands, ordered by similarity to the input string
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<Command> FuzzyCommandSearch(string cmd) => Commands.OrderBy(x => cmd.ToLower().DistanceFrom(x.Name.ToLower()));

        #region Run

        private static void Run(InputCommand cmd)
        {
            Command exec = Commands[cmd.Name.ToLower()];
            if (exec == null)
            {
                //fuzzy command suggestions
                var suggestedCommand = FuzzyCommandSearch(cmd.Name).FirstOrDefault();
                if (suggestedCommand != null)
                {
                    CLI.Out("Did you mean ");
                    CLI.Out(suggestedCommand.Name, ConsoleColor.Cyan);
                    if (cmd.Arguments.Count > 0) CLI.Out(" ");
                    
                    foreach (var a in cmd.Arguments)
                        CLI.Out($"-{a.Key} {a.Value} ", ConsoleColor.DarkCyan);
                    CLI.Out("? ");
                    CLI.Line("[Y/n]", ConsoleColor.Green);

                    Console.ForegroundColor = ConsoleColor.Green; //not using CLI.Out(ConsoleColor) because I want the user input to also be Cyan
                    CLI.Out(InputPrefix);

                    if (Console.ReadLine() == "Y") exec = suggestedCommand;
                    else exec = Commands["help"];

                    Console.ResetColor();
                }
                else exec = Commands["help"];
            }

            var missingRequiredArgs = exec.RequiredArgs.Select(x => x.Tag.ToLower()).Where(x => !cmd.Arguments.ContainsKey(x.ToLower())).ToList();
            if (missingRequiredArgs.Count > 0)
            {
                //fuzzy argument suggestions
                if(cmd.Arguments.Count > 0)
                {
                    CLI.Out("Did you mean ");
                    CLI.Out(exec.Name + " ", ConsoleColor.DarkCyan);

                    //input arguments that don't exist in the Command definition, mapped to include suggested alternatives
                    var nonexistentArgs =
                        cmd.Arguments
                            .Where(x =>
                                !exec.RequiredArgs
                                    .Select(y => y.Tag.ToLower())
                                    .Contains(x.Key.ToLower())
                                )
                            .Select(x => new
                                {
                                    Original = x,
                                    Suggestion = exec.RequiredArgs.OrderBy(y => y.Tag.ToLower().DistanceFrom(x.Key.ToLower())).First()
                                })
                        .ToList();

                    //input arguments that do exist in the Command definition
                    var existentArgs =
                        cmd.Arguments
                            .Where(x =>
                                exec.RequiredArgs
                                    .Select(y => y.Tag.ToLower())
                                    .Contains(x.Key.ToLower())
                                );

                    foreach (var n in nonexistentArgs)
                        CLI.Out($"-{n.Suggestion.Tag} {n.Original.Value} ", ConsoleColor.Cyan);
                    foreach (var e in existentArgs)
                        CLI.Out($"-{e.Key} {e.Value} ", ConsoleColor.DarkCyan);
                    CLI.Out("? ");
                    CLI.Line("[Y/n]", ConsoleColor.Green);

                    Console.ForegroundColor = ConsoleColor.Green; //not using CLI.Out(ConsoleColor) because I want the user input to also be Cyan
                    CLI.Out(InputPrefix);
                    if (Console.ReadLine() == "Y")
                    {
                        foreach (var arg in nonexistentArgs)
                        {
                            var inArg = cmd.Arguments.First(x => x.Key == arg.Original.Key);
                            cmd.Arguments.Remove(inArg.Key);
                            cmd.Arguments.Add(arg.Suggestion.Tag, inArg.Value);
                        }
                    }
                    else exec = Commands["help"];

                    Console.ResetColor();
                }
                else exec = Commands["help"];
            }


            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                exec.Function.Invoke(cmd.Arguments);
                sw.Stop();
                CLI.Line(sw.ElapsedMilliseconds + "ms");
            }
            catch (Exception e)
            {
                sw.Stop();
                CLI.Line(e.Message, ConsoleColor.Red);
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