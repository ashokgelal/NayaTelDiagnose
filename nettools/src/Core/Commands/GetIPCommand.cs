using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace nettools.Core.Commands
{

    internal class GetIPCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string hostname = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address", "host", "hostname");

            if (string.IsNullOrEmpty(hostname) || string.IsNullOrWhiteSpace(hostname))
            {
                Console.WriteLine("Please enter a hostname");
                return false;
            }

            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions(99, true);
            PingReply reply = default(PingReply);

            string formatter = " {0,-6} | {1,-40} | {2,-30} | {3,-10}";

            int timeout = 1000;
            byte[] buffer = Encoding.ASCII.GetBytes("NetToolsGetIPData");

            Console.WriteLine(formatter, "Online", "IP-Address", "Ping-Status", "Roundtrip");
            Console.WriteLine(formatter, new string('=', 6), new string('=', 40), new string('=', 30), new string('=', 10));

            foreach (IPAddress ip in Dns.GetHostEntry(hostname).AddressList)
            {
                try
                {
                    reply = pinger.Send(ip, timeout, buffer, pingerOptions);

                    if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                    {
                        Console.WriteLine(formatter, "Yes", ip.ToString(), reply.Status, reply.RoundtripTime + "ms");
                        Logger.Debug("Ping to " + ip.ToString() + " successful (" + reply.Status + ")", "GetIP");
                    }
                    else
                    {
                        Console.WriteLine(formatter, "No", ip.ToString(), reply.Status);
                        Logger.Debug("Ping to " + ip.ToString() + " failed: " + reply.Status, "GetIP");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug("Exception while pinging " + ip.ToString() + ": " + ex.Message + " (" + ex.Source + ")", "GetIP");
                }
            }

            return true;
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "getip", "resolveip" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "a*0", "addr|a", "address|a", "host|a", "hostname|a" };
        }

        public string GetHelp()
        {
            return "Get the IP of the given hostname";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "a")
                return "The hostname of which the IP should be resolved";
            else
                return null;
        }

        public void Stop() { }

    }

}
