using System;
using IgniteCLI;

namespace Sample
{
    class Program
    {
        static void Main(string[] a)
        {
            CLI.Start(new CommandList
            {
                new Command
                {
                    Name = "Test",
                    Args = new CommandArgs
                    {
                        new CommandArg
                        {
                            Tag = "arg",
                            InputFormat = "value",
                            Required = false,
                        },
                    },
                    Function = args =>
                    {
                        int x = 0;
                    }
                }
            });
        }
    }
}
