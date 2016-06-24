using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace nettools.Core.Commands
{

    internal class IPInfoCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string ipAddress = ArgumentParser.StripArgument(arguments, "", 0, "ip", "a", "addr", "address");

            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrWhiteSpace(ipAddress))
            {
                Console.WriteLine("Please enter a IP-Address");
                return false;
            }

            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions(1, true);
            int timeout = 1000;
            byte[] buffer = Encoding.ASCII.GetBytes("NetToolsIPInfoData");
            PingReply reply = default(PingReply);

            reply = pinger.Send(ipAddress, timeout, buffer, pingerOptions);

            string hostname = "";
            string[] aliases = new string[] { "" };

            try
            {
                hostname = Dns.GetHostEntry(ipAddress).HostName;
            }
            catch (Exception) { }

            try
            {
                aliases = Dns.GetHostEntry(ipAddress).Aliases;
            }
            catch (Exception) { }

            Console.WriteLine(" Infos about \"" + ipAddress + "\"");
            Console.WriteLine("\tPing-Status: " + reply.Status.ToString());
            Console.WriteLine("\tResolved hostname: " + hostname);
            Console.WriteLine("\tResolved aliases: " + String.Join(", ", aliases));

            return true;
        }
        
        public string[] GetAvailableCommands()
        {
            return new string[] { "ipinfo", "ipinf" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "ip*0", "a|ip", "addr|ip", "address|ip" };
        }

        public string GetHelp()
        {
            return "Print informations about the given IP";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "ip")
                return "The IP which should be scanned";
            else
                return null;
        }

        public void Stop() { }

    }

}
