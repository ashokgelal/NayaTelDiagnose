using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using nettools.Plugins;

namespace nettools.Core
{

    internal class CommandExecutor
    {

        private static Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

        public static void LoadPluginCommands(List<Tuple<IPlugin, IPluginCommand>> commands)
        {
            foreach (Tuple<IPlugin, IPluginCommand> command in commands)
                LoadPluginCommand(command.Item1, command.Item2);
        }

        public static void LoadPluginCommand(IPlugin plugin, IPluginCommand command)
        {
            Logger.Debug("Loading command \"" + command.Name + "\" from plugin \"" + plugin.Name + "\"...", "Loader");
            commands.Add(command.Name, command);
        }

        public static void LoadCommands()
        {
            Logger.Debug("Loading commands...", "Loader");
            Type[] commandTypes = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "nettools.Core.Commands");

            foreach(Type command in commandTypes)
            {
                Logger.Debug("\tLoading \"" + command.Name + "\"", "Loader");
                ICommand instance = (ICommand)Activator.CreateInstance(command);
                commands.Add(command.Name, instance);
            }

            Logger.Debug("Commands loaded.", "Loader");
        }

        public static ICommand ExecuteCommand(string command, Dictionary<string, string> arguments)
        {
            foreach(ICommand cmdInst in commands.Values)
            {
                if(cmdInst.GetAvailableCommands().Any(t => t == command.ToLower()))
                {
                    //cmdInst.Execute(arguments);
                    return cmdInst;
                }
            }

            Console.WriteLine("Unknown command. Enter \"help\" for a list of all commands");
            return null;
        }

        public static void GetHelp(string text, bool pagedHelp = false)
        {
            Console.WriteLine();

            IOrderedEnumerable<KeyValuePair<string, ICommand>> sortedCommand = commands.OrderBy(t => t.Key);

            foreach (KeyValuePair<string, ICommand> item in sortedCommand)
            {
                ICommand cmdInst = item.Value;

                if(pagedHelp)
                    Console.Clear();

                string[] commands = cmdInst.GetAvailableCommands();
                string[] arguments = cmdInst.GetAvailableArguments();

                if (text != "")
                    if (!arguments.Any((t => t.ToLower().Contains(text.ToLower()))) && !commands.Any((t => t.ToLower().Contains(text.ToLower()))) && !cmdInst.GetHelp().Contains(text))
                        continue;         

                List<string> argumentsList = new List<string>();
                Dictionary<string, string[]> commandsAliasesList = new Dictionary<string, string[]>();
                SortedDictionary<int, string> commandsIndexesList = new SortedDictionary<int, string>();
                
                foreach (string str in arguments)
                {
                    if(str.Contains("|"))
                    {
                        if(commandsAliasesList.ContainsKey(str.Split('|')[1]))
                        {
                            List<string> tmpAliases = new List<string>(commandsAliasesList[str.Split('|')[1]]);
                            tmpAliases.Add("--" + str.Split('|')[0]);

                            commandsAliasesList.Remove(str.Split('|')[1]);
                            commandsAliasesList.Add(str.Split('|')[1], tmpAliases.ToArray());

                            tmpAliases = null;
                        }
                        else
                        {
                            commandsAliasesList.Add(str.Split('|')[1], new string[] { "-" + str.Split('|')[0] });
                        }
                    }
                    else if (str.Contains("*"))
                    {
                        string arg = str.Split('*')[0];
                        int id = int.Parse(str.Split('*')[1].TrimEnd('!'));
                        
                        if (commandsIndexesList.ContainsKey(id))
                        {
                            string currValue = commandsIndexesList[id];
                            currValue += ";" + arg;

                            commandsIndexesList.Remove(id);
                            commandsIndexesList.Add(id, currValue);
                        }
                        else
                            commandsIndexesList.Add(id, arg);

                        argumentsList.Add(arg);
                    }
                    else
                    {
                        argumentsList.Add(str);
                    }
                }

                string indexArguments = "";
                
                for (int i = 0; i < commandsIndexesList.Count; i++)
                {
                    string argument = commandsIndexesList[i];

                    if (argument.Contains(";"))
                    {
                        string tmp = "[";

                        foreach(string tmpArg in argument.Split(';'))
                            tmp += cmdInst.GetArgumentHelp(tmpArg) + " / ";

                        indexArguments += ((i != 0) ? new string(' ', 11) : "") + tmp.TrimEnd(' ', '/') + "]\n";
                    }
                    else
                    {
                        indexArguments += ((i != 0) ? new string(' ', 11) : "") + "[" + cmdInst.GetArgumentHelp(argument) + "]\n";
                    }
                }
                
                if (commands.Length > 1)
                    Console.Write(commands[0] + ": " + cmdInst.GetHelp() + "\nArguments: " + indexArguments.TrimEnd(' ', ',') + "\n" +
                        "\t(Aliases: " + string.Join(", ", commands, 1, commands.Length - 1) + ")\n");
                else
                    Console.Write(commands[0] + ": " + cmdInst.GetHelp() + "\nArguments: " + indexArguments.TrimEnd(' ', ',') + "\n");

                foreach (string argument in argumentsList)
                {
                    if(commandsAliasesList.ContainsKey(argument))
                        Console.WriteLine("\t-" + argument + ": " + cmdInst.GetArgumentHelp(argument) + "\n\t\tAliases: " + string.Join(", ", commandsAliasesList[argument]));
                    else
                        Console.WriteLine("\t-" + argument + ": " + cmdInst.GetArgumentHelp(argument));
                }

                Console.WriteLine("\n");
                if (pagedHelp)
                {
                    Console.Write("Press any key to go to the next command...");
                    Console.ReadKey();
                }
            }
            if (pagedHelp)
                Console.Clear();
        }

        private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal) && t.GetInterfaces().Any(i => i.Name == "ICommand")).ToArray();
        }

    }

}
