using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace nettools.Core.Commands
{

    internal class MTRCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string hostOrIpAddress = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address", "ip", "host", "hostname");
            int timeout = ArgumentParser.StripArgument(arguments, 1000, "t", "timeout");
            int waiting = ArgumentParser.StripArgument(arguments, 500, "w", "wait");
            bool resolveHostname = ArgumentParser.StripArgument(arguments, true, "r", "reslv", "resolve");

            if (string.IsNullOrEmpty(hostOrIpAddress) || string.IsNullOrWhiteSpace(hostOrIpAddress))
            {
                Console.WriteLine("Please enter a hostname or an ip-address");
                return false;
            }

            IEnumerable<IPAddress> routes = TracerouteAddress(hostOrIpAddress, 1);
            Dictionary<IPAddress, int> consoleTops = new Dictionary<IPAddress, int>();
            Dictionary<IPAddress, string> routeHostnames = new Dictionary<IPAddress, string>();

            string formatter = " {0,-3} | {1,-6} | {2,-44} | {3,-5} | {4,-4} | {5,-5} | {6,-5} | {7,-5} | {8,-4} | {9,-6}";
            string formatterHead = " {0,-3} | {1,-6} | {2,-44} | {3,-5} | {4,-4} | {5,-5} | {6,-5} | {7,-5} | {8,-4} | {9,-6}";
            string titleFormatter = " {0,-36}   {1,18}   {2,53}";

            Console.CursorVisible = false;

            Console.WriteLine(titleFormatter, "", "MTR [nettools v0.1]", "");
            Console.WriteLine(titleFormatter, hostOrIpAddress, "", DateTime.Now.ToString("MMM ddd d HH:mm:ss yyyy"));
            Console.WriteLine();

            Console.WriteLine(formatterHead, "#", "Count", "Hostname/IP-Address", "Loss", "Rcv", "Sent", "Last", "Best", "Avg", "Worst");
            Console.WriteLine(formatter, new string('=', 3), new string('=', 6), new string('=', 44), new string('=', 5), new string('=', 4), 
                new string('=', 5), new string('=', 5), new string('=', 5), new string('=', 4), new string('=', 6));

            int count = 1;

            foreach (IPAddress route in routes)
            {
                if (route == null)
                {
                    Console.WriteLine(formatter, count, 0, "Unknown", 0, 0, 0, 0, 0, 0);
                }
                else
                {
                    if (resolveHostname)
                    {
                        try
                        {
                            string host = Dns.GetHostEntry(route).HostName;

                            Console.WriteLine(formatter, 0, count, host, 0, 0, 0, 0, 0, 0, 0);
                            routeHostnames.Add(route, host);
                        }
                        catch(SocketException)
                        {
                            Console.WriteLine(formatter, count, 0, route.ToString(), 0, 0, 0, 0, 0, 0, 0);
                            routeHostnames.Add(route, route.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine(formatter, count, 0, route.ToString(), 0, 0, 0, 0, 0, 0, 0);
                    }
                }

                count++;
                consoleTops.Add(route, Console.CursorTop - 1);

                Console.WriteLine(formatter, new string('-', 3), new string('-', 6), new string('-', 44), new string('-', 5), new string('-', 4),
                    new string('-', 5), new string('-', 5), new string('-', 5), new string('-', 4), new string('-', 6));
            }

            int loopCount = 1;

            Dictionary<IPAddress, int> pings_sent = new Dictionary<IPAddress, int>();
            Dictionary<IPAddress, int> pings_rcv = new Dictionary<IPAddress, int>();
            Dictionary<IPAddress, int> pings_last = new Dictionary<IPAddress, int>();
            Dictionary<IPAddress, int> pings_best = new Dictionary<IPAddress, int>();
            Dictionary<IPAddress, int> pings_worst = new Dictionary<IPAddress, int>();
            Dictionary<IPAddress, List<int>> pings_values = new Dictionary<IPAddress, List<int>>();
            
            while (loopCount < int.MaxValue)
            {
                int loopInternalCount = 1;

                foreach (IPAddress route in routes)
                {
                    Console.CursorTop = consoleTops[route];
                    string host = (routeHostnames.ContainsKey(route) ? routeHostnames[route] : route.ToString());

                    Ping pinger = new Ping();
                     
                    AddOrIncreaseDictionary(pings_sent, route);
                    PingReply reply = pinger.Send(route, timeout);

                    if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                    {
                        AddOrIncreaseDictionary(pings_rcv, route);
                    }

                    AddOrSetDictionary(pings_last, route, (int)reply.RoundtripTime);

                    if (pings_values.ContainsKey(route))
                    {
                        pings_values[route].Add((int)reply.RoundtripTime);
                    }
                    else
                    {
                        pings_values.Add(route, new List<int>( new int[] { (int)reply.RoundtripTime } ));
                    }

                    if (pings_best.ContainsKey(route))
                    {
                        if (reply.RoundtripTime < pings_best[route])
                        {
                            AddOrSetDictionary(pings_best, route, (int)reply.RoundtripTime);
                        }
                    }
                    else
                        AddOrSetDictionary(pings_best, route, (int)reply.RoundtripTime);

                    if (pings_worst.ContainsKey(route))
                    {
                        if (reply.RoundtripTime > pings_worst[route])
                        {
                            AddOrSetDictionary(pings_worst, route, (int)reply.RoundtripTime);
                        }
                    }
                    else
                        AddOrSetDictionary(pings_worst, route, (int)reply.RoundtripTime);
                    
                    // Console.WriteLine(formatterHead, "Hostname/IP-Address", "Loss", "Rcv", "Sent", "Last", "Best", "Avg", "Worst");
                    Console.WriteLine(formatter, loopInternalCount, loopCount, host, (((pings_sent[route] - GetOrReturnDefault(pings_rcv, route)) / pings_sent[route]) * 100),
                       GetOrReturnDefault(pings_rcv, route), pings_sent[route], pings_last[route], 
                        pings_best[route], Math.Round(pings_values[route].Average(), 0), pings_worst[route]);

                    loopInternalCount++;
                }

                loopCount++;
            }

            return true;
        }

        private void AddOrIncreaseDictionary(Dictionary<IPAddress, int> dic, IPAddress key)
        {
            if (dic.ContainsKey(key))
            {
                int value = dic[key];
                value++;
                dic.Remove(key);
                dic.Add(key, value);
            }
            else
            {
                dic.Add(key, 1);
            }
        }

        private int GetOrReturnDefault(Dictionary<IPAddress, int> dic, IPAddress key, int def = 0)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            else
            {
                return def;
            }
        }

        private void AddOrSetDictionary(Dictionary<IPAddress, int> dic, IPAddress key, int value)
        {
            if (dic.ContainsKey(key))
            {
                dic.Remove(key);
                dic.Add(key, value);
            }
            else
            {
                dic.Add(key, value);
            }
        }

        private static IEnumerable<IPAddress> TracerouteAddress(string hostNameOrAddress, int ttl, string sendData = "NetToolsTracerouteData")
        {
            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions(ttl, true);
            PingReply reply = default(PingReply);
            
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
                result.Add(reply.Address);
            }
            else if (reply.Status == IPStatus.TtlExpired)
            {
                if (reply.Status == IPStatus.TtlExpired)
                    result.Add(reply.Address);
                                
                IEnumerable<IPAddress> tempResult = default(IEnumerable<IPAddress>);
                tempResult = TracerouteAddress(hostNameOrAddress, ttl + 1, sendData);
                result.AddRange(tempResult);
            }

            return result;
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "mtr", "trceping" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "a*0", "addr|a", "address|a", "ip|a", "host|a", "hostname|a", "c", "t", "timeout|t", "r", "reslv|r", "resolve|r" };
        }

        public string GetHelp()
        {
            return "Ping the given IP/Hostname";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "a")
                return "The hostname/IP-Address which should be watched";
            else if (arg == "t")
                return "The timeout of a ping";
            else if (arg == "r")
                return "Indicates if the hostname of each IP should be resolved";
            else if (arg == "w")
                return "Pause between pings in milliseconds";
            else
                return null;
        }

        public void Stop() { }

    }

}
