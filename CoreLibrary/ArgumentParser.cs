using System;
using System.Collections.Generic;
using System.Linq;

namespace nettools.Core
{

    internal class ArgumentParser
    {

        public static Tuple<string, Dictionary<string, string>> Parse(string input)
        {
            Tuple<string, string[]> splitted = SplitCommand(input);
            return new Tuple<string, Dictionary<string, string>>(splitted.Item1.ToLower(), ParseArguments(splitted.Item2));
        }

        public static T StripArgument<T>(Dictionary<string, string> dict, T defaultValue, params string[] arguments)
        {
            return StripArgument(dict, defaultValue, -1, arguments);
        }

        public static T StripArgument<T>(Dictionary<string, string> dict, T defaultValue, int position, params string[] arguments)
        {
            if (position != -1 && position < dict.Count)
                return (T)Convert.ChangeType(dict.Keys.ToArray()[position], typeof(T));

            foreach(string arg in arguments)
            {
                if (dict.ContainsKey(arg))
                    return (T)Convert.ChangeType(dict[arg], typeof(T));
            }

            return defaultValue;
        }

        private static Dictionary<string, string> ParseArguments(string[] arguments)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            for (int i = 0; i < arguments.Length; i++)
            {
                string item = arguments[i];
                bool isCommandInit = false;

                if (item.StartsWith("-") || item.StartsWith("--"))
                    isCommandInit = true;

                if (isCommandInit)
                {
                    string _command = item.Split(new char[] { ' ' }, 2)[0];
                    string _value = "";

                    if (arguments.Length > (i + 1))
                        _value = arguments[i + 1];
                    else
                        _value = "true";

                    if (_value.StartsWith("-") || _value.StartsWith("--"))
                    {
                        if (!args.ContainsKey(item))
                            args.Add(_command.Replace("-", ""), "");
                    }
                    else
                    {
                        if (!args.ContainsKey(item))
                            args.Add(_command.Replace("-", ""), _value);
                        i++;
                    }
                }
                else
                {
                    if(!args.ContainsKey(item) && !string.IsNullOrWhiteSpace(item))
                        args.Add(item, "true");
                }
            }

            return args;
        }

        private static Tuple<string, string[]> SplitCommand(string input)
        {
            if (!input.Contains(" "))
                return new Tuple<string, string[]>(input.ToLower(), new string[] { "" } );

            string command = input.Substring(0, input.IndexOf(' '));
            string args = input.Substring(input.IndexOf(' '), input.Length - input.IndexOf(' ')).Substring(1);

            return new Tuple<string, string[]>(command, GetCommandLineArgs(args));
        }

        private static string[] GetCommandLineArgs(string input)
        {
            /* int argc;

            IntPtr argv = NativeMethods.CommandLineToArgvW(input, out argc);
            if (argv == IntPtr.Zero)
                return new string[0];

            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            } */
            return nettools.ThirdParty.CmdLineTokenizer.Tokenizer.TokenizeCommandLineToStringArray(input);
        }

    }

}
