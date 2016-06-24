using System;
using System.Collections.Generic;

using nettools.Utils;

namespace nettools.Core.Commands
{

    internal class WgetCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string fileUrl = ArgumentParser.StripArgument(arguments, "", 0, "f", "file", "url");
            string method = ArgumentParser.StripArgument(arguments, "GET", "m", "method");
            int timeout = ArgumentParser.StripArgument(arguments, 10000, "t", "timeout");

            string authUser = ArgumentParser.StripArgument(arguments, "", "u", "user");
            string authPass = ArgumentParser.StripArgument(arguments, "", "p", "pass", "password");
                        
            if (string.IsNullOrEmpty(fileUrl) || string.IsNullOrWhiteSpace(fileUrl))
            {
                Logger.Warning("At least a vaild URL to a file has to be given", "WGet");
            }

            DownloadUtils.DownloadFile(new Uri(fileUrl), timeout, method, authUser, authPass);

            return true;
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "wget", "dl", "download" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "f*0", "file|f", "url|f", "m", "method|m", "t", "timeout|t", "u", "user|u", "p", "pass|p", "password|p" };
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
