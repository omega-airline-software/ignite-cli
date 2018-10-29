using System;
using IgniteCLI;

namespace Sample
{
    class Program
    {
        static void Main(string[] a)
        {
            CLI.Start(new Commands
            {
                new Command
                {
                    Name = "Test",
                    Args = new CommandArgs
                    {
                        new CommandArg
                        {
                            Tag = "aa",
                            InputFormat = "value",
                            Required = true,
                        },
                        new CommandArg
                        {
                            Tag = "bb",
                            Required = true
                        },
                    },
                    Function = args =>
                    {
                        CLI.Line(CLI.String(args, "aa"));
                        CLI.Line(CLI.String(args, "bb"));
                        int[] i = new int[0];
                        CLI.Line($"{i[1]}");
                    }
                }
            });
        }
    }
}
