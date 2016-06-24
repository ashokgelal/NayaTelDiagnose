using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using nettools.Properties;
using nettools.Utils;

namespace nettools.Core.Commands
{

    internal class WhoIsCommand : ICommand
    {

        Dictionary<string, string> tld_whois_servers = new Dictionary<string, string>();

        public WhoIsCommand()
        {
            string[] servers = Resources.whoisservers.Split('\n');
            foreach (string line in servers)
            {
                if (line.StartsWith(";"))
                    continue;

                tld_whois_servers.Add(line.Split(' ')[0], line.Split(' ')[1]);
            }
        }

        public bool Execute(Dictionary<string, string> arguments)
        {
            string domain = ArgumentParser.StripArgument(arguments, "", 0, "d", "domain");
            bool raw = ArgumentParser.StripArgument(arguments, false, "r", "raw");

            if (domain.Length == 0 || domain.Length > 255 || !Regex.IsMatch(domain, @"(?:[A-Za-z0-9][A-Za-z0-9\-]{0,61}[A-Za-z0-9]|[A-Za-z0-9])"))
            {
                Console.WriteLine("Please enter a valid domain");
                return false;
            }

            string whois_server = "";

            foreach (KeyValuePair<string, string> entry in tld_whois_servers)
            {
                if (domain.EndsWith("." + entry.Key))
                    whois_server = entry.Value;
            }

            if (string.IsNullOrEmpty(whois_server) || string.IsNullOrWhiteSpace(whois_server))
            {
                Console.WriteLine("No whois-server found for that tld...");
                return false;
            }

            TcpTransmitter tcp = new TcpTransmitter(whois_server.Trim('\r', '\n', '\0'), 43);

            tcp.WriteLine(domain);
            tcp.WaitForAvailbility();
            string whois = tcp.ReadToEnd();

            if (raw)
            {
                Console.Write("\n" + whois);
            }
            else
            {
                string[] lines = whois.Split('\n');
                foreach (string _line in lines)
                {
                    string line = _line.TrimEnd('\r');
                    if (!line.StartsWith("%"))
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            return true;
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "whois" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "d*0", "domain|d", "r", "raw|r" };
        }

        public string GetHelp()
        {
            return "Query the whois-server of the given domain.";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "d")
                return "The domain which should be queried";
            else if (arg == "r")
                return "Indicates if the full query should be returned";
            else
                return null;
        }

        public void Stop() { }

    }

}
