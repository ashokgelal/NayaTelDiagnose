using System;
using System.Collections.Generic;
using System.Net;

namespace nettools.Core.Commands
{

    internal class ARPPingCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string ipAddress = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address", "ip");

            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrWhiteSpace(ipAddress))
            {
                Console.WriteLine("Please enter an ip-address");
                return false;
            }

            ARPPing(ipAddress);

            return true;
        }
        
        private static void ARPPing(string ipAddress)
        {
            byte[] macAddr = new byte[6];
            int macAddrLen = macAddr.Length;

            DateTime startTime = DateTime.Now;
            Logger.Debug("Sending ARP-Packet", "ARPPing");
            int arpResult = NativeMethods.SendARP(BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0), 0, macAddr, ref macAddrLen);
            DateTime endTime = DateTime.Now;
            TimeSpan time = endTime.Subtract(startTime);

            bool arpSuccess = arpResult == 0;

            if (arpSuccess)
            {
                Logger.Debug("ARP success!", "ARPPing");
                string[] macArr = new string[macAddrLen];
                for (int mac = 0; mac < macAddrLen; mac++)
                    macArr[mac] = macAddr[mac].ToString("X2");

                string macStr = string.Join(":", macArr);

                Console.WriteLine("\tResolved MAC-Address: " + macStr);
                Console.WriteLine("\tRoundtrip-Time: " + time.TotalMilliseconds + " ms");
            }
            else
            {
                Logger.Debug("ARP failed, result: " + arpResult, "ARPPing");
                Console.WriteLine("Failed to send ARP to " + ipAddress);
            }
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "arpping", "arp" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "a*0", "addr|a", "address|a", "ip|a" };
        }

        public string GetHelp()
        {
            return "Send a ARP-Packet to the given IP-Address";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "a")
                return "The IP to which the ARP-Packet should be sent";
            else
                return null;
        }

        public void Stop() { }

    }

}
