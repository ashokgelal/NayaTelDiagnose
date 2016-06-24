using System.Collections.Generic;
using System.Net;

namespace nettools.Core.Commands
{

    internal class ProxyCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string address = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address");

            string authUser = ArgumentParser.StripArgument(arguments, "", 1, "u", "user");
            string authPass = ArgumentParser.StripArgument(arguments, "", 2, "p", "pass", "password");

            if (address.ToLower() == "reset")
            {
                Program.__proxy = null;
                return true;
            }

            if (string.IsNullOrEmpty(address) || string.IsNullOrWhiteSpace(address))
            {
                Logger.Warning("At least a vaild address to the proxy has to be given");
            }

            if(!string.IsNullOrEmpty(authUser) && !string.IsNullOrWhiteSpace(authUser))
                Program.__proxy = new WebProxy(address, true, new string[] { }, new NetworkCredential(authUser, authPass));
            else
                Program.__proxy = new WebProxy(address, true, new string[] { });

            return true;
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "proxy" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "a", "addr|a", "address|a", "u", "user|u", "p", "pass|p", "password|p" };
        }

        public string GetHelp()
        {
            return "Download a file to the current directory";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "f")
                return "The URL of the file";
            else if (arg == "m")
                return "The method which should be used to download the file";
            else if (arg == "t")
                return "Duration in milliseconds after which the connection should time out";
            else if (arg == "u")
                return "Username which should be used to login (Optional)";
            else if (arg == "p")
                return "Password which should be used to login (Optional)";
            else
                return null;
        }

        public void Stop() { }

    }

}
