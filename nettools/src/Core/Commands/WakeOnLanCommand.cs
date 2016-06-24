using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace nettools.Core.Commands
{

    internal class WakeOnLanCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string macString = ArgumentParser.StripArgument(arguments, "", 0, "m", "mac", "a", "addr", "address");

            if (string.IsNullOrEmpty(macString) || string.IsNullOrWhiteSpace(macString))
            {
                Console.WriteLine("Please enter an MAC-Address");
                return false;
            }

            try
            {
                byte[] mac = GetMacArray(macString);
                SendWOL(mac);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Please enter an valid MAC-Address");
            }

            return true;
        }

        private static void SendWOL(byte[] mac)
        {
            Console.WriteLine("Starting sending WOL-Packet...");

            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Broadcast, 40000);

            byte[] packet = new byte[17 * 6];

            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];

           /* int result = */ client.Send(packet, packet.Length);

            Console.WriteLine("WOL-Packet sent!");
        }
        
        public static byte[] GetMacArray(string mac)
        {
            if (string.IsNullOrEmpty(mac)) throw new ArgumentNullException("mac");
            byte[] ret = new byte[6];
            try
            {
                string[] tmp = mac.Split(':', '-');
                if (tmp.Length != 6)
                {
                    tmp = mac.Split('.');
                    if (tmp.Length == 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            ret[i * 2] = byte.Parse(tmp[i].Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                            ret[i * 2 + 1] = byte.Parse(tmp[i].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        }
                    }
                    else
                        for (int i = 0; i < 12; i += 2)
                            ret[i / 2] = byte.Parse(mac.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                }
                else
                    for (int i = 0; i < 6; i++)
                        ret[i] = byte.Parse(tmp[i], System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                throw new ArgumentException("Argument doesn't have the correct format: " + mac, "mac");
            }
            return ret;
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "wakeonlan", "wol", "wakeon", "wakeup" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "m*0", "mac|m", "a|m", "addr|m", "address|m" };
        }

        public string GetHelp()
        {
            return "Wake up the device with the given MAC-Address";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "m")
                return "The MAC-Address of the device which should be woken up";
            else
                return null;
        }

        public void Stop() { }

    }

}
