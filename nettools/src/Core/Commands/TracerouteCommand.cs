using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace nettools.Core.Commands
{

    internal class TracerouteCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string hostOrIpAddress = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address", "ip", "host", "hostname");
            bool resolvePointHostname = ArgumentParser.StripArgument(arguments, false, "r", "reslv", "resolve");

            if (string.IsNullOrEmpty(hostOrIpAddress) || string.IsNullOrWhiteSpace(hostOrIpAddress))
            {
                Console.WriteLine("Please enter a hostname or an ip-address");
                return false;
            }

            TracerouteAddress(hostOrIpAddress, resolvePointHostname);

            return true;
        }

        public static IEnumerable<IPAddress> TracerouteAddress(string hostNameOrAddress, bool resolveHostName = false, string sendData = "NetToolsTracerouteData")
        {
            string formatter = " {0,-6} | {1,-40} | {2,-35}";

            Console.WriteLine(formatter, "Jump", "IP-Address", "Hostname");
            Console.WriteLine(formatter, new string('=', 6), new string('=', 40), new string('=', 35));

            return TracerouteAddress(hostNameOrAddress, 1, resolveHostName);
        }

        private static IEnumerable<IPAddress> TracerouteAddress(string hostNameOrAddress, int ttl, bool resolveHostName = false, string sendData = "NetToolsTracerouteData")
        {
            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions(ttl, true);
            PingReply reply = default(PingReply);

            string formatter = " {0,-6} | {1,-40} | {2,-35}";
            int timeout = 1000;
            byte[] buffer = Encoding.ASCII.GetBytes(sendData);

            try
            {
                reply = pinger.Send(hostNameOrAddress, timeout, buffer, pingerOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return default(IEnumerable<IPAddress>);
            }

            List<IPAddress> result = new List<IPAddress>();
            if (reply.Status == IPStatus.Success)
            {
                string ipStr = reply.Address.ToString();
                string hostStr = "";

                try
                {
                    if (resolveHostName)
                        hostStr = Dns.GetHostEntry(reply.Address).HostName;
                }
                catch (Exception) { }

                Console.WriteLine(formatter, ttl, ipStr, hostStr);

                result.Add(reply.Address);
            }
            else if (reply.Status == IPStatus.TtlExpired)
            {
                result.Add(reply.Address);

                string ipStr = ((reply.Address == null) ? "Unknown" : reply.Address.ToString());
                string hostStr = "";

                try
                {
                    if (resolveHostName)
                        hostStr = Dns.GetHostEntry(reply.Address).HostName;
                }
                catch (Exception) { }

                Console.WriteLine(formatter, ttl, ipStr, hostStr);

                IEnumerable<IPAddress> tempResult = default(IEnumerable<IPAddress>);
                tempResult = TracerouteAddress(hostNameOrAddress, ttl + 1, resolveHostName, sendData);
                result.AddRange(tempResult);
            }
            else
            {
                string ipStr = ((reply.Address == null) ? "Unknown" : reply.Address.ToString());
                string hostStr = "";

                try
                {
                    if (resolveHostName)
                        hostStr = Dns.GetHostEntry(reply.Address).HostName;
                }
                catch (Exception) { }

                Console.WriteLine(formatter, ttl, ipStr, hostStr);
            }

            return result;
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "traceroute", "tr", "route", "trace" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "a*0", "addr|a", "address|a", "ip|a", "host|a", "hostname|a", "r", "reslv|r", "resolve|r" };
        }

        public string GetHelp()
        {
            return "Traceroute to the given address";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "a")
                return "The address which should be routed";
            else if (arg == "r")
                return "If set, the hostname of the routed IP will be resolved";
            else
                return null;
        }

        public void Stop() { }

    }

}
