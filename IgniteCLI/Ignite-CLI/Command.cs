using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IgniteCLI
{
    public class CommandList : List<Command>
    {
        public Command this[string s]
        {
            get { return this.FirstOrDefault(x => x.Name == s || x.Aliases.Contains(s)); }
        }
    }

    public class Command
    {
        public delegate void CommandFunc(Dictionary<string, string> cmdArgs);

        public string Name;
        public string[] Aliases;
        public string Description;
        public CommandArgs Args = new CommandArgs();
        public CommandFunc Function;

        public List<CommandArg> RequiredArgs => Args.Where(x => x.Required).ToList();
        public string Format()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var arg in Args)
            {
                if (!arg.Required)
                    sb.Append("{");

                sb.Append($"-{arg.Tag}");
                if (arg.InputFormat != null)
                    sb.Append($" [{arg.InputFormat}]");

                if (!arg.Required)
                    sb.Append("}");

                sb.Append(" ");
            }
            return sb.ToString();
        }
    }

    public class CommandArgs : List<CommandArg>
    {
        public CommandArg this[string s]
        {
            get { return this.FirstOrDefault(x => x.Tag == s || x.Aliases.Contains(s)); }
        }
    }

    public class CommandArg
    {
        public string Tag;
        public string[] Aliases;
        public string Description;
        public string InputFormat;
        public bool Required = true;
    }
}
