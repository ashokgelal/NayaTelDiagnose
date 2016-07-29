using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace nettools.Core.Commands
{

    public class MTRParallel
    {
        public delegate void mtrResult(int loopInternalCount, int loopCount, string host, float Loss,
                      int Recv, int sent, int last, 
                        int best, double avg, int worst, int index);
        public delegate void mtrRoutes(IEnumerable<IPAddress> routes);
        bool isStop = false;

        private static Boolean stop = true;
        public bool Execute(string hostOrIpAddress, float hopTimeout, int pingTimeout, int timeout, int max_ttl, int iteration_interval,int iterations_per_host,int packet_size , mtrRoutes mtrRoutesRes,mtrResult mtr)
        {
            // string hostOrIpAddress = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address", "ip", "host", "hostname");
            // int timeout = ArgumentParser.StripArgument(arguments, 1000, "t", "timeout");
            // int waiting = ArgumentParser.StripArgument(arguments, 500, "w", "wait");
            //bool resolveHostname = ArgumentParser.StripArgument(arguments, true, "r", "reslv", "resolve");

           
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
           // int hop_discovery_timeout = PropertyProvider.getPropertyProvider().getMTRProperty(PropertyProvider.NetworkResponse.IterationInterval);
            IEnumerable<IPAddress> routes = TracerouteAddressWithMaxHopParallel(hostOrIpAddress, max_ttl, hopTimeout);

            System.Timers.Timer upTimer = new System.Timers.Timer(timeout);
            upTimer.Elapsed += (s, e) =>
            {
                Stop();
            };
            upTimer.AutoReset = false;
            upTimer.Enabled = true;



            Dictionary<IPAddress, string> routeHostnames = new Dictionary<IPAddress, string>();
 
            int count = 0;
            mtrRoutesRes(routes);

            Dictionary<String, MTRIPStatus> pings_status = new Dictionary<String, MTRIPStatus>();

            int keyIndex = 0;
            foreach (IPAddress route in routes)
            {
                

                    MTRIPStatus IPResponse = new MTRIPStatus();
                    IPResponse.timeList = new List<int>();
                    IPResponse.Loss = "0";
                   String strRoute = route != null ? route.ToString() : "*";
                    pings_status.Add(keyIndex+"_"+ strRoute, IPResponse);
                    keyIndex++;  
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
                int index = -1;
                // double unixTime = span.TotalSeconds;

                Debug.WriteLine("NetWorkResponse: Time:" + (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds + " Starting Ping Iteration"); // 2

                foreach (IPAddress route in routes)
                {
                    index++;
                    if (route == null)
                    {
                        
                        continue;
                        //Console.WriteLine(formatter, count, 0, "Unknown", 0, 0, 0, 0, 0, 0);
                    }
                  
                    Ping ping = new Ping();
                    String keyes = index + "_" + route;
                    MTRIPStatus Response = pings_status[keyes];
                    Response.Sent++;
                    Response.IPAdress =  route.ToString();
                    Response.Key = index;
                    //AddOrIncreaseDictionary(pings_sent, keyes);
                    ping.SendAsync(route, pingTimeout, new byte[] { 0 }, Response);                   
                    ping.PingCompleted += (s, e) =>
                    {
                        MTRIPStatus Ping_Response = e.UserState as MTRIPStatus;
                        //Console.CursorTop = consoleTops[route];
                        //string host = (routeHostnames.ContainsKey(route) ? routeHostnames[route] : route.ToString());
                        String ipAddress = Response.IPAdress;
                        string host = ipAddress;
                        PingReply reply = e.Reply;

                        if (reply == null)
                        {

                        }

                        if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                        {
                            Ping_Response.Rec++;
                            //AddOrIncreaseDictionary(pings_rcv, key);
                        }
                        Ping_Response.Last = (int)reply.RoundtripTime;

                        Ping_Response.timeList.Add((int)reply.RoundtripTime);

                        // AddOrSetDictionary(pings_last, key, (int)reply.RoundtripTime);

                        // if (pings_values.ContainsKey(key))
                        // {
                        //    pings_values[key].Add((int)reply.RoundtripTime);
                        ///}
                        // else
                        // {
                        //     pings_values.Add(key, new List<int>(new int[] { (int)reply.RoundtripTime }));
                        // }



                        if (reply.RoundtripTime < Ping_Response.Best || Ping_Response.Best == 0)
                        {
                            Ping_Response.Best = (int)reply.RoundtripTime;
                        }
                         



                        if (reply.RoundtripTime > Ping_Response.Worst)
                            {
                              Ping_Response.Worst = (int)reply.RoundtripTime;
                            }


                        // Console.WriteLine(formatterHead, "Hostname/IP-Address", "Loss", "Rcv", "Sent", "Last", "Best", "Avg", "Worst");
                        //Console.WriteLine(formatter, loopInternalCount, loopCount, host, (((pings_sent[route] - GetOrReturnDefault(pings_rcv, route)) / pings_sent[route]) * 100),
                        //   GetOrReturnDefault(pings_rcv, route), pings_sent[route], pings_last[route], 
                        //    pings_best[route], Math.Round(pings_values[route].Average(), 0), pings_worst[route]);
                        float minus = (Ping_Response.Sent - Ping_Response.Rec);
                        float perc = (minus / Ping_Response.Sent) * 100;
                     
                        Ping_Response.Loss = perc.ToString();
                        mtr(loopInternalCount, loopCount, host, perc,
                         Ping_Response.Rec, Ping_Response.Sent, Ping_Response.Last,
                             Ping_Response.Best, Math.Round(Ping_Response.timeList.Average()), Ping_Response.Worst, Ping_Response.Key);
                        


                    };

                    loopInternalCount++;
                   // index++;
                   
                }
                 Debug.WriteLine("NetWorkResponse: Time:" + (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds + " End Ping Iteration waiting for :"+ iteration_interval ); // 2

                Thread.Sleep(iteration_interval);
                Debug.WriteLine("NetWorkResponse: Time:" + (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds + " Wait Completed  "  ); // 2

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
        public IEnumerable<IPAddress> TracerouteAddressWithMaxHopParallel(string ipAddress, int maxHops, float timeout)
        {
            timeout = (timeout-500)/2;
            int pingTime = int.Parse(timeout.ToString());
            List<IPAddress> result = new List<IPAddress>();
            Dictionary<int, String> hopsTtlDic = new Dictionary<int, String>();
             List<int> PingTimeOut = new List<int>();

            
            bool isHopsTimeOut = false;
            for (int i = 0; i < maxHops; i++)
            {
                hopsTtlDic.Add(i + 1, "*");
            }
            //    System.Timers.Timer upTimer = new System.Timers.Timer(timeout);
            //upTimer.Elapsed += (s, e) =>
            //{
            //    //isHopsTimeOut = true;
            //};
            //upTimer.AutoReset = false;
            //upTimer.Enabled = true;


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


            
           
            // int ttl = 0;
            PingOptions pingOptions = new PingOptions(1, true);
            int countResponse = 0;
            Boolean destinationIpFound = false; 
            for (int i = 0; i < maxHops; i++)
            {
                Thread.Sleep(500/maxHops);
                TraceLocation _TraceLocation = new TraceLocation();
                _TraceLocation.IpAddress = ipAddress;
                _TraceLocation.TTL = pingOptions.Ttl;
                Ping ping = new Ping();
                ping.SendAsync(address, pingTime, new byte[] { 0 }, pingOptions, _TraceLocation);
                pingOptions.Ttl++;
              
                ping.PingCompleted += (s,e)=>
                { 
                    countResponse++;
                    TraceLocation traceLocation   = ((TraceLocation)e.UserState);
                    int ttl = traceLocation.TTL;
                    Debug.WriteLine("Ping Completed: ttl: " + ttl);

                    // int ttl =   e.UserState.  ;
                    if (e.Reply.Address != null && e.Reply.Status == IPStatus.TtlExpired)
                    {
                        result.Add(e.Reply.Address);
                        hopsTtlDic[ttl] = e.Reply.Address.ToString();
 
                    }
                    else if (e.Reply.Address != null && e.Reply.Status == IPStatus.Success && ipAddress == e.Reply.Address.ToString())
                    {
                        hopsTtlDic[ttl] = "**";

                        if (!destinationIpFound)
                        {

                            if (!destinationIpFound && !hopsTtlDic.ContainsValue(ipAddress.ToString()))
                            {
                                hopsTtlDic[ttl] = ipAddress.ToString();
                            }
                            destinationIpFound = true;
                        }

                    } else if (e.Reply.Status == IPStatus.TimedOut &&  !PingTimeOut.Contains(ttl)){
                        Debug.WriteLine("Ping Time Out For Hops: ttl: "+ttl);

                        PingTimeOut.Add(ttl);
                        Ping pingRetry = s as Ping; 
                        pingRetry.SendAsync(traceLocation.IpAddress, pingTime, new byte[] { 0 }, new PingOptions(ttl, true), traceLocation);
                        countResponse--;
                        hopsTtlDic[ttl] = "*";
                    }
                    if (countResponse == maxHops)
                    {  
                          isHopsTimeOut = true;
                    }
                };

                // Return out TracertEntry object with all the information about the hop.
                //yield return new TracertEntry()
                //{
                //    HopID = pingOptions.Ttl,
                //    Address = reply.Address == null ? "N/A" : reply.Address.ToString(),
                //    Hostname = hostname,
                //    ReplyTime = pingReplyTime.ElapsedMilliseconds,
                //    ReplyStatus = reply.Status
                //};

            }
            while (!isHopsTimeOut)
            {
                Thread.Sleep(50);
            }


            List<IPAddress> Hopresult = new List<IPAddress>();
            Dictionary<int, String>  Dic = new Dictionary<int, String>(hopsTtlDic);

            foreach (var item in Dic)
            {
                IPAddress Hop;

                if (item.Value == "**" || item.Value == "0.0.0.0")
                    continue;
                if (IPAddress.TryParse(item.Value, out Hop))
                {
                    Hopresult.Add(Hop);

                }
                else {
                    Hopresult.Add(null);

                }
            }

           // var sortList = from element in Hopresult
                      //    orderby element == null
                      //    select element;
            //while (reply.Status != IPStatus.Success && pingOptions.Ttl <= maxHops);
            return Hopresult;
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
    public class TraceLocation
    {
        /// <summary>
        /// Hop number in a particular trace.
        /// </summary>
        public int Hop { get; set; }
        /// <summary>
        /// Time in milliseconds.
        /// </summary>
        public long Time { get; set; }
        /// <summary>
        /// IP address returned.
        /// </summary>
        public String IpAddress { get; set; }

        /// <summary>
        /// TTL address returned.
        /// </summary>
        public int TTL { get; set; }

    }


    public class MTRIPStatus
    {
        public  int Count { get; set; }
        public string IPAdress { get; set; }
        public string Loss { get; set; }
        public int Rec { get; set; }
        public int Sent { get; set; }
        public int Last { get; set; }
        public int Best { get; set; }
        public int Avg { get; set; }
        public int Worst { get; set; }
        public int Index { get; set; }
        public int Key { get; set; }
        public List<int> timeList;
    }
}
