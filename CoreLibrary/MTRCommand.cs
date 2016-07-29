using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace nettools.Core.Commands
{

    public class MTRCommand 
    {
        public delegate void mtrResult(int loopInternalCount, int loopCount, string host, float Loss,
                      int Recv, int sent, int last, 
                        int best, double avg, int worst, int index);
        public delegate void mtrRoutes(IEnumerable<IPAddress> routes);
        bool isStop = false;

        private static Boolean stop = true;
        public bool Execute(string hostOrIpAddress, int pingTimeout, int timeout, int max_ttl, int iteration_interval,int iterations_per_host,int packet_size , mtrRoutes mtrRoutesRes,mtrResult mtr)
        {
            // string hostOrIpAddress = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address", "ip", "host", "hostname");
            // int timeout = ArgumentParser.StripArgument(arguments, 1000, "t", "timeout");
            // int waiting = ArgumentParser.StripArgument(arguments, 500, "w", "wait");
            //bool resolveHostname = ArgumentParser.StripArgument(arguments, true, "r", "reslv", "resolve");

            System.Timers.Timer upTimer = new System.Timers.Timer(timeout);
            upTimer.Elapsed += (s, e) =>
            {
                Stop();
             };
            upTimer.AutoReset = false;
            upTimer.Enabled = true;






            String data = "";
            for (int i = 0; i < packet_size; i++)
            {
                data += "a";
            }
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            bool resolveHostname = false;
            if (string.IsNullOrEmpty(hostOrIpAddress) || string.IsNullOrWhiteSpace(hostOrIpAddress))
            {
                Console.WriteLine("Please enter a hostname or an ip-address");
                return false;
            }

            IEnumerable<IPAddress> routes = TracerouteAddressWithMaxHop(hostOrIpAddress, max_ttl, pingTimeout);
             Dictionary<IPAddress, string> routeHostnames = new Dictionary<IPAddress, string>();

            //string formatter = " {0,-3} | {1,-6} | {2,-44} | {3,-5} | {4,-4} | {5,-5} | {6,-5} | {7,-5} | {8,-4} | {9,-6}";
            //string formatterHead = " {0,-3} | {1,-6} | {2,-44} | {3,-5} | {4,-4} | {5,-5} | {6,-5} | {7,-5} | {8,-4} | {9,-6}";
            //string titleFormatter = " {0,-36}   {1,18}   {2,53}";

 
            //Console.WriteLine(titleFormatter, "", "MTR [nettools v0.1]", "");
            //Console.WriteLine(titleFormatter, hostOrIpAddress, "", DateTime.Now.ToString("MMM ddd d HH:mm:ss yyyy"));
            //Console.WriteLine();

            //Console.WriteLine(formatterHead, "#", "Count", "Hostname/IP-Address", "Loss", "Rcv", "Sent", "Last", "Best", "Avg", "Worst");
            //Console.WriteLine(formatter, new string('=', 3), new string('=', 6), new string('=', 44), new string('=', 5), new string('=', 4), 
            //    new string('=', 5), new string('=', 5), new string('=', 5), new string('=', 4), new string('=', 6));

            int count = 0;
            mtrRoutesRes(routes);
            foreach (IPAddress route in routes)
            {
                if (route == null)
                {
                    //Console.WriteLine(formatter, count, 0, "Unknown", 0, 0, 0, 0, 0, 0);
                }
                else
                {
                    if (false)
                    {
                        try
                        {
                            string host = Dns.GetHostEntry(route).HostName;

                            //Console.WriteLine(formatter, 0, count, host, 0, 0, 0, 0, 0, 0, 0);
                            routeHostnames.Add(route, host);
                        }
                        catch(SocketException)
                        {
                            //Console.WriteLine(formatter, count, 0, route.ToString(), 0, 0, 0, 0, 0, 0, 0);
                            routeHostnames.Add(route, route.ToString());
                        }
                    }
                    else
                    {
                       // Console.WriteLine(formatter, count, 0, route.ToString(), 0, 0, 0, 0, 0, 0, 0);
                    }
                }

                count++;
               // consoleTops.Add(route, Console.CursorTop - 1);

                //Console.WriteLine(formatter, new string('-', 3), new string('-', 6), new string('-', 44), new string('-', 5), new string('-', 4),
                //    new string('-', 5), new string('-', 5), new string('-', 5), new string('-', 4), new string('-', 6));
            }

            int loopCount = 1;

            Dictionary<String, int> pings_sent = new Dictionary<String, int>();
            Dictionary<String, int> pings_rcv = new Dictionary<String, int>();
            Dictionary<String, int> pings_last = new Dictionary<String, int>();
            Dictionary<String, int> pings_best = new Dictionary<String, int>();
            Dictionary<String, int> pings_worst = new Dictionary<String, int>();
            Dictionary<String, List<int>> pings_values = new Dictionary<String, List<int>>();
            
            while (loopCount <= iterations_per_host & !isStop)
            {
                int loopInternalCount = 1;
                int index = 0;

                foreach (IPAddress route in routes)
                {
                    String key = index + "_" + route;
                    //Console.CursorTop = consoleTops[route];
                    string host = (routeHostnames.ContainsKey(route) ? routeHostnames[route] : route.ToString());

                    Ping pinger = new Ping();
                     
                    AddOrIncreaseDictionary(pings_sent, key);
                    PingReply reply = pinger.Send(route, pingTimeout, buffer);

                    if (reply == null) {

                    }

                    if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                    {
                        AddOrIncreaseDictionary(pings_rcv, key);
                    }

                    AddOrSetDictionary(pings_last, key, (int)reply.RoundtripTime);

                    if (pings_values.ContainsKey(key))
                    {
                        pings_values[key].Add((int)reply.RoundtripTime);
                    }
                    else
                    {
                        pings_values.Add(key, new List<int>( new int[] { (int)reply.RoundtripTime } ));
                    }

                    if (pings_best.ContainsKey(key))
                    {
                        if (reply.RoundtripTime < pings_best[key])
                        {
                            AddOrSetDictionary(pings_best, key, (int)reply.RoundtripTime);
                        }
                    }
                    else
                        AddOrSetDictionary(pings_best, key, (int)reply.RoundtripTime);

                    if (pings_worst.ContainsKey(key))
                    {
                        if (reply.RoundtripTime > pings_worst[key])
                        {
                            AddOrSetDictionary(pings_worst, key, (int)reply.RoundtripTime);
                        }
                    }
                    else
                        AddOrSetDictionary(pings_worst, key, (int)reply.RoundtripTime);
                    
                    // Console.WriteLine(formatterHead, "Hostname/IP-Address", "Loss", "Rcv", "Sent", "Last", "Best", "Avg", "Worst");
                    //Console.WriteLine(formatter, loopInternalCount, loopCount, host, (((pings_sent[route] - GetOrReturnDefault(pings_rcv, route)) / pings_sent[route]) * 100),
                    //   GetOrReturnDefault(pings_rcv, route), pings_sent[route], pings_last[route], 
                    //    pings_best[route], Math.Round(pings_values[route].Average(), 0), pings_worst[route]);
                    mtr(loopInternalCount, loopCount, host, (((pings_sent[key] - GetOrReturnDefault(pings_rcv, key)) / pings_sent[key]) * 100),
                      GetOrReturnDefault(pings_rcv, key), pings_sent[key], pings_last[key],
                         pings_best[key], Math.Round(pings_values[key].Average(), 0), pings_worst[key],index);
                    loopInternalCount++;
                    index++;
                }

                Thread.Sleep(iteration_interval);
                loopCount++;
            }

            return true;
        }
        public   void  stopMtr(){
             stop = false;
            }
        private void AddOrIncreaseDictionary(Dictionary<String, int> dic, String key)
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

        private int GetOrReturnDefault(Dictionary<String, int> dic, String key, int def = 0)
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

        private void AddOrSetDictionary(Dictionary<String, int> dic, String key, int value)
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
        /// <summary>
        /// Traces the route which data have to travel through in order to reach an IP address.
        /// </summary>
        /// <param name="ipAddress">The IP address of the destination.</param>
        /// <param name="maxHops">Max hops to be returned.</param>
        public IEnumerable<IPAddress> TracerouteAddressWithMaxHop(string ipAddress, int maxHops, int timeout)
        {
            IPAddress address;

            // Ensure that the argument address is valid.
            if (!IPAddress.TryParse(ipAddress, out address))
                throw new ArgumentException(string.Format("{0} is not a valid IP address.", ipAddress));

            // Max hops should be at least one or else there won't be any data to return.
            if (maxHops < 1)
                throw new ArgumentException("Max hops can't be lower than 1.");

            // Ensure that the timeout is not set to 0 or a negative number.
            if (timeout < 1)
                throw new ArgumentException("Timeout value must be higher than 0.");


            Ping ping = new Ping();
            PingOptions pingOptions = new PingOptions(1, true);
            Stopwatch pingReplyTime = new Stopwatch();
            PingReply reply;
            List<IPAddress> result = new List<IPAddress>();

            do
            {
                pingReplyTime.Start();
                reply = ping.Send(address, timeout, new byte[] { 0 }, pingOptions);
                pingReplyTime.Stop();

                string hostname = string.Empty;
                 if (reply.Address != null)
                {
                     result.Add(reply.Address);
                }
                  
               // Return out TracertEntry object with all the information about the hop.
               //yield return new TracertEntry()
               //{
               //    HopID = pingOptions.Ttl,
               //    Address = reply.Address == null ? "N/A" : reply.Address.ToString(),
               //    Hostname = hostname,
               //    ReplyTime = pingReplyTime.ElapsedMilliseconds,
               //    ReplyStatus = reply.Status
               //};

                pingOptions.Ttl++;
                pingReplyTime.Reset();
            }
            while (reply.Status != IPStatus.Success && pingOptions.Ttl <= maxHops);
            return result;
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

        public void Stop() {

            isStop = true;

        }

    }

}
