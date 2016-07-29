using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ViewSwitchingNavigation.Infrastructure.Utils
{
    public class UtilNetwork
    {

        public static bool isConectedWifi() {


            bool IsConnected = NetworkInterface.GetIsNetworkAvailable();
            return IsConnected;

        }
        public static async Task<string> GetMachineNameFromIPAddress(string ipAdress)
        {
            string machineName = string.Empty;
            try
            {
                //IPHostEntry hostEntry = Dns.GetHostEntry(ipAdress);
                var hostEntry = await Dns.GetHostEntryAsync(ipAdress);
                machineName = hostEntry.HostName;
            }
            catch (Exception ex)
            {
                machineName = "UNKNOWN";
            }
            return machineName;
        }

        public static String GetMacAddress(byte[] mac)
        {
            byte[] macAddr = mac;
            string tMac = "";
            for (int i = 0; i < macAddr.Length; i++)
            {

                tMac += macAddr[i].ToString("x2").PadLeft(2, '0').ToUpper() + ":";

            }
            tMac = tMac.Substring(0, tMac.Length - 1);
            return tMac;
        }
        public static String GetMacAddress(String mac)
        {
            if (String.IsNullOrEmpty(mac))
                return mac;
            var output = string.Join(":", Enumerable.Range(0, 6)
    .Select(i => mac.Substring(i * 2, 2)));



            return output;
        }
        public static String GetLocalMacAddress()
        {
            var macAddr =
     (from nic in NetworkInterface.GetAllNetworkInterfaces()
      where nic.OperationalStatus == OperationalStatus.Up
      select nic.GetPhysicalAddress().ToString()).FirstOrDefault();


            return GetMacAddress(macAddr);
        }

        public static bool isConnectedInternet(int timeout)
        {
            try
            {
                Ping myPing = new Ping();
                String host = "google.com";
                byte[] buffer = new byte[32];
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer);
                if (reply == null)
                    return false;
                if (reply.Status == IPStatus.Success)
                {
                    return true;

                }
                else
                {
                    return false;

                }
            }
            catch (Exception)
            {

                return false;
            }

        }

        public static IPAddress GetSubnetMask(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }
            throw new ArgumentException(string.Format("Can't find subnetmask for IP address '{0}'", address));
        }
        public static string GetLocalDeviceName()
        {
            return Environment.MachineName;

        }

        public static string GetLocalIPAddress()
        {



            WlanClient client = new WlanClient();


            var ipv4 = "";
            int count = 0;
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {


                Wlan.WlanConnectionAttributes WlanConnection;
                try
                {
                    WlanConnection = wlanIface.CurrentConnection;
                }
                catch (Exception)
                {

                    continue;
                }
                count++;
                if (count == 2)
                    break;
                UnicastIPAddressInformationCollection UnicastIPInfoCol = wlanIface.NetworkInterface.GetIPProperties().UnicastAddresses;
                foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol)
                {
                    if (UnicatIPInfo.Address.IsIPv6LinkLocal)
                    {

                        // wifiInfo.IPv6 = UnicatIPInfo.Address.ToString();


                    }
                    else
                    {
                        ipv4 = UnicatIPInfo.Address.ToString();
                    }
                }

            }

            if (String.IsNullOrEmpty(ipv4)) {

                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if ((ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet) && (ni.OperationalStatus == OperationalStatus.Up))
                    {
                        //Console.WriteLine(ni.Name);
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                ipv4 = ip.Address.ToString() ;
                            }
                        }
                    }
                }
            }



            return ipv4;

            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (var ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        return ip.ToString();
            //    }
            //}
            //throw new Exception("Local IP Address Not Found!");
        }

        public static List<IPAddress> GetDefualtWayIpAddress() {
            WlanClient client = new WlanClient();
 
            List<IPAddress> IpList = new List<IPAddress>();

            try
            {
                int count = 0;
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    count++;
                    Wlan.WlanConnectionAttributes WlanConnection;
                     try
                    {
                        WlanConnection = wlanIface.CurrentConnection;
                    }
                    catch (Exception)
                    {

                        continue;
                    }

                    NetworkInterface Interface = wlanIface.NetworkInterface;
                    GatewayIPAddressInformationCollection gateway_address = Interface.GetIPProperties().GatewayAddresses;
                    IPAddress DefualtGatewayAddress = null;
                    foreach (var address in gateway_address)
                    {
                        if (address.Address.IsIPv6LinkLocal)
                        {
                        }
                        else
                        {
                            DefualtGatewayAddress = address.Address;
                            IpList.Add(DefualtGatewayAddress);
                        }
                    }
                    
                   
                    
                  

                }
            }
            catch (Exception ex)
            {
            }
            return IpList;
        }

        // Signals when the resolve has finished.
        public static ManualResetEvent GetHostEntryFinished =
            new ManualResetEvent(false);

        // Record the IPs in the state object for later use.
        public static void GetHostEntryCallback(IAsyncResult ar)
        {
            ResolveState ioContext = (ResolveState)ar.AsyncState;
            try
            {
                ioContext.IPs = Dns.EndGetHostEntry(ar);

            }
            catch (Exception)
            {

                //throw;
            }
            GetHostEntryFinished.Set();
        }

        // Determine the Internet Protocol (IP) addresses for 
        // this host asynchronously.
        public static void DoGetHostEntryAsync(string hostname)
        {
            GetHostEntryFinished.Reset();
            ResolveState ioContext = new ResolveState(hostname);

            Dns.BeginGetHostEntry(ioContext.host,
                new AsyncCallback(GetHostEntryCallback), ioContext);

            // Wait here until the resolve completes (the callback 
            // calls .Set())
            GetHostEntryFinished.WaitOne();

            Console.WriteLine("EndGetHostEntry({0}) returns:", ioContext.host);

            foreach (IPAddress ip in ioContext.IPs.AddressList)
            {
                Console.WriteLine("    {0}", ip);
            }

        }

    }


    // Define the state object for the callback. 
    // Use hostName to correlate calls with the proper result.
    public class ResolveState
    {
        string hostName;
        IPHostEntry resolvedIPs;

        public ResolveState(string host)
        {
            hostName = host;
        }

        public IPHostEntry IPs
        {
            get { return resolvedIPs; }
            set { resolvedIPs = value; }
        }
        public string host
        {
            get { return hostName; }
            set { hostName = value; }
        }
    }

    public static class SubnetMask
    {
        public static readonly IPAddress ClassA = IPAddress.Parse("255.0.0.0");
        public static readonly IPAddress ClassB = IPAddress.Parse("255.255.0.0");
        public static readonly IPAddress ClassC = IPAddress.Parse("255.255.255.0");

        public static IPAddress CreateByHostBitLength(int hostpartLength)
        {
            int hostPartLength = hostpartLength;
            int netPartLength = 32 - hostPartLength;

            if (netPartLength < 2)
                throw new ArgumentException("Number of hosts is to large for IPv4");

            Byte[] binaryMask = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                if (i * 8 + 8 <= netPartLength)
                    binaryMask[i] = (byte)255;
                else if (i * 8 > netPartLength)
                    binaryMask[i] = (byte)0;
                else
                {
                    int oneLength = netPartLength - i * 8;
                    string binaryDigit =
                        String.Empty.PadLeft(oneLength, '1').PadRight(8, '0');
                    binaryMask[i] = Convert.ToByte(binaryDigit, 2);
                }
            }
            return new IPAddress(binaryMask);
        }

        public static IPAddress CreateByNetBitLength(int netpartLength)
        {
            int hostPartLength = 32 - netpartLength;
            return CreateByHostBitLength(hostPartLength);
        }

        public static IPAddress CreateByHostNumber(int numberOfHosts)
        {
            int maxNumber = numberOfHosts + 1;

            string b = Convert.ToString(maxNumber, 2);

            return CreateByHostBitLength(b.Length);
        }
    }

    public static class IPAddressExtensions
    {
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }
    }
}
