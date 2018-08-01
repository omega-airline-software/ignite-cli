using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IgniteCLI
{
    public class CLI
    {
        #region Convenience Functions
        public static string String(Dictionary<string, string> d, string key) => d.ContainsKey(key.ToLower()) ? d[key.ToLower()] : null;
        public static int Int(Dictionary<string, string> d, string key) => Convert.ToInt32(CLI.String(d, key));
        public static bool Bool(Dictionary<string, string> d, string key) => d.ContainsKey(key.ToLower()) ? d[key.ToLower()] == "true" : false;
        #endregion

        private static CommandList Commands;

        public static void Start(CommandList commands)
        {
            Commands = commands;
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Write("> ");

            var input = Console.ReadLine();
            while (input != "exit")
            {
                string cmd = "help";
                var cmdArgs = new Dictionary<string, string>();
                if (input.Contains(" "))
                {
                    try
                    {
                        cmd = input.Substring(0, input.IndexOf(" "));
                        var tokens = input.Substring(input.IndexOf(" ") + 2).Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var t in tokens)
                        {
                            if (t.Contains(' '))
                                cmdArgs.Add(t.Substring(0, t.IndexOf(' ')).ToLower(), t.Substring(t.IndexOf(' ') + 1));
                            else
                                cmdArgs.Add(t, "true");
                        }
                    }
                    catch { }
                }
                else cmd = input;

                if (input.Length != 0)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    Command exec = Commands[cmd.ToLower()] ?? Commands["help"];
                    if (!exec.RequiredArgs.Select(x => x.Tag).All(x => cmdArgs.ContainsKey(x)))
                        exec = Commands["help"];
                    try
                    {
                        exec.Function.Invoke(cmdArgs);
                        sw.Stop();
                        CLI.Out(sw.ElapsedMilliseconds + "ms");
                    }
                    catch (Exception e)
                    {
                        sw.Stop();
                        CLI.Out(e.Message, ConsoleColor.Red);
                        CLI.Help();
                    }

                    CLI.Out();
                }
                Console.Write("> ");
                input = Console.ReadLine();
            }
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
            Out("HELP:");
            Out("cmd -arg [value] {-optionalArg [optional value]} {-optionalBool}");
            Break();
            foreach (var cmd in Commands)
            {
                Out($"{cmd.Name} {cmd.Format()}", ConsoleColor.Green);
                Out($"# {cmd.Description}", ConsoleColor.Cyan);
                foreach (var arg in cmd.Args)
                {
                    Out($"| {arg.Tag} : {arg.Description}", ConsoleColor.DarkCyan);
                }
                Out();
            }
            Break();
        }

        public static void Run(string command, params string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            foreach (string s in args)
            {
                var ss = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                arguments.Add(ss[0], ss[1]);
            }
            Commands[command].Function.Invoke(arguments);
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
