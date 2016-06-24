using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace nettools.Core.Commands
{

    internal class PingCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string hostOrIpAddress = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address", "ip", "host", "hostname");
            string sendData = ArgumentParser.StripArgument(arguments, "pingpong", "d", "data");
            int count = ArgumentParser.StripArgument(arguments, int.MaxValue, "c", "count");
            int timeout = ArgumentParser.StripArgument(arguments, 1000, "t", "timeout");
            int wait = ArgumentParser.StripArgument(arguments, 500, "w", "wait");

            if (string.IsNullOrEmpty(hostOrIpAddress) || string.IsNullOrWhiteSpace(hostOrIpAddress))
            {
                Console.WriteLine("Please enter a hostname or an ip-address");
                return false;
            }

            Ping(hostOrIpAddress, count, timeout, wait, sendData);

            return true;
        }

        private static void Ping(string hostNameOfAddress, int count, int timeout, int wait, string sendData = "NetToolsPingData")
        {
            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions();

            byte[] buffer = Encoding.ASCII.GetBytes(sendData);
            string formatter = " {0,-6} | {1,-55} | {2,-35} | {3,-15} | {4,-6}";
            
            Console.WriteLine(formatter, "Count", "Address", "Status", "Roundtrip-Time", "Ttl");
            Console.WriteLine(formatter, new string('=', 6), new string('=', 55), new string('=', 35), new string('=', 15), new string('=', 6));

            int successCount = 0;
            double roundtrip = 0;

            for (int i = 0; i < count; i++)
            {
                PingReply reply = null;
                try
                {
                    reply = pinger.Send(hostNameOfAddress, timeout, buffer, pingerOptions);
                }
                catch (ThreadAbortException) { break; }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                    continue;
                }

                if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                    successCount++;

                if (reply.Options == null)
                    Console.WriteLine(formatter, i + 1, "Ping to " + hostNameOfAddress + " failed", "", "", "");
                else
                    Console.WriteLine(formatter, i + 1, reply.Address, reply.Status, reply.RoundtripTime + " ms", reply.Options.Ttl);

                roundtrip += (reply.RoundtripTime / 2);

                Thread.Sleep(wait);
            }

            Console.WriteLine();

            Console.WriteLine("Stats: ");
            Console.WriteLine("\tTotal: " + count);
            Console.WriteLine("\tSuccess: " + successCount);
            Console.WriteLine("\tSucceeded: " + Math.Round((100.0 * successCount) / count, 0) + "%");
            Console.WriteLine("\tAverage Roundtrip: " + Math.Round(roundtrip / count, 0) + " ms");
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "ping" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "a*0", "addr|a", "address|a", "ip|a", "host|a", "hostname|a", "c", "count|c", "w", "wait|w", "t", "timeout|t", "d", "data|d" };
        }

        public string GetHelp()
        {
            return "Ping the given IP/Hostname";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "a")
                return "The hostname/IP-Address which should be pinged";
            else if (arg == "c")
                return "The amount of pings which should be sent";
            else if (arg == "w")
                return "Time in milliseconds between to pings";
            else if (arg == "t")
                return "The timeout of a ping";
            else if (arg == "d")
                return "The data which should be sent";
            else
                return null;
        }

        public void Stop() { }

    }

}
