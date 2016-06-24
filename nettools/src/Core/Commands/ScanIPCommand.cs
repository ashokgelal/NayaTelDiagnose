using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

using nettools.Utils;

namespace nettools.Core.Commands
{

    internal class ScanIPCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string startIP = "";
            string endIP = "";

            bool resolveHostname = ArgumentParser.StripArgument(arguments, false, "reslv", "resolve", "host");

            if (!arguments.ContainsKey("s") && !arguments.ContainsKey("start"))
            {
                string definedRange = ArgumentParser.StripArgument(arguments, "", 0, "r", "range");

                if (definedRange == "local")
                {
                     startIP = "192.168.1.1";
                     endIP = "192.168.1.100";
                     //startIP = "192.168.1.7";
                      //endIP = "192.168.7.254";
                }
                else if (definedRange == "full")
                {
                    startIP = "0.0.0.0";
                    endIP = "255.255.255.255";
                }
                else
                {
                    Console.WriteLine("Please enter a valid range");
                    return false;
                }
            }
            else
            {
                startIP = ArgumentParser.StripArgument(arguments, "", -1, "s", "start");
                endIP = ArgumentParser.StripArgument(arguments, "", -1, "e", "end");
            }

            if ((string.IsNullOrEmpty(startIP) || string.IsNullOrWhiteSpace(startIP)) || (string.IsNullOrEmpty(endIP) || string.IsNullOrWhiteSpace(endIP)))
            {
                Console.WriteLine("Please enter a start and an end of the range");
                return false;
            }

            try
            {
                ScanIPRange(startIP, endIP, resolveHostname);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, LogMethod.FileOnly);
            }

            return true;
        }
        
        private static void ScanIPRange(string start, string end, bool resolve)
        {
            ScanIPRange(IPAddress.Parse(start), IPAddress.Parse(end), resolve);
        }

        private static Dictionary<IPAddress, string> ScanIPRange(IPAddress start, IPAddress end, bool resolve)
        {
            Dictionary<IPAddress, string> ipMacDir = new Dictionary<IPAddress, string>();
            string formatter = " {0,-25} | {1,-30} | {2,-25} | {3,-6}";

            if(!resolve)
                formatter = " {0,-25} | {1,-25} | {2,-6}";

            int currStatusTop = 0;
            int currTableTop = 0;

            currStatusTop = Console.CursorTop;

            Console.WriteLine("\n\n=== Scan Results: IP-ADDRESSES ===\n");
            if (resolve)
            {
                Console.WriteLine(formatter, "IP-Address", "Hostname", "MAC", "Ping");
                Console.WriteLine(formatter, new string('=', 25), new string('=', 30), new string('=', 25), new string('=', 6));
            }
            else
            {
                Console.WriteLine(formatter, "IP-Address", "MAC", "Ping");
                Console.WriteLine(formatter, new string('=', 25), new string('=', 25), new string('=', 6));
            }
            currTableTop = Console.CursorTop;

            foreach (IPAddress ip in IPUtils.GetIPRange(start, end))
            {
                Console.CursorTop = currStatusTop;
                Console.Write("Scanning " + ip.ToString() + "...\r");

                Ping pinger = new Ping();
                PingReply pingReply = pinger.Send(ip, 3000);

                if (pingReply.Status == IPStatus.Success || pingReply.Status == IPStatus.TtlExpired)
                {
                    byte[] macAddr = new byte[6];
                    int macAddrLen = macAddr.Length;

                    int arpResult = NativeMethods.SendARP(BitConverter.ToInt32(ip.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen);
                    bool arpSuccess = arpResult == 0;

                    if (arpSuccess)
                    {
                        string[] macArr = new string[macAddrLen];
                        for (int mac = 0; mac < macAddrLen; mac++)
                            macArr[mac] = macAddr[mac].ToString("X2");

                        string ipStr = ip.ToString();
                        string hostStr = "";
                        string macStr = string.Join(":", macArr);

                        if (resolve)
                        {
                            try
                            {
                                hostStr = Dns.GetHostEntry(ip).HostName;
                            }
                            catch (Exception) { }
                        }

                        ipMacDir.Add(ip, macStr);
                        Console.CursorTop = currTableTop;
                        currTableTop++;

                        if (resolve)
                        {
                            Console.WriteLine(formatter, ipStr, hostStr, macStr, pingReply.RoundtripTime + "ms");
                        }
                        else
                        {
                            Console.WriteLine(formatter, ipStr, macStr, pingReply.RoundtripTime + "ms");
                        }
                    }
                }
            }

            return ipMacDir;
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "scanip", "ipscan" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "r*0", "range|r", "s*1" , "start|s", "e*1", "end|e", "reslv", "resolve|reslv", "host|reslv" };
        }

        public string GetHelp()
        {
            return "Scanning a IP-Range";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "r")
                return "Scan a pre-defined IP-Range (Available: local, full)";
            else if (arg == "s")
                return "The start of the IP-Range";
            else if (arg == "e")
                return "The end of the IP-Range";
            else if (arg == "reslv")
                return "Iniciates if the hostname of the IP should be resolved";
            else
                return null;
        }

        public void Stop() { }

    }

}
