using NativeWifi;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ViewSwitchingNavigation.Infrastructure;
using WifiInspectorModule.Model;
using static NativeWifi.Wlan;

namespace WifiInspectorModule.Services
{
    [Export(typeof(IWifiInfoService))]
    class WifiInfoService : IWifiInfoService
    {

          /// <summary>
        /// Supported 2.4Ghz 5Ghz  family list
        /// </summary>
        /// 
        String[] SUPPORTED_CHANNELS_5_GHZ = new string[] {"802.11b", "802.11n" , "802.11ac" };
        String[] SUPPORTED_CHANNELS_2_5_GHZ = new string[] { "802.11a", "802.11g", "802.11n" };


        private IEnumerable<WifiInfo> getWifiList()
        {



            WlanClient client = new WlanClient();
            List<WifiInfo> wifiConeectedInfoList = new List<WifiInfo>();

            List<String> slistMac = new List<string>();

            try
            {
                int count = 0;
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    count++;

                    NetworkInterfaceType type = wlanIface.NetworkInterface.NetworkInterfaceType;
                    //if(wlanIface.CurrentConnection.wlanConnectionMode == WlanConnect)
                    
                    Wlan.WlanConnectionAttributes WlanConnection;
                    try
                    {
                        WlanConnection = wlanIface.CurrentConnection;

                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    WifiInfo wifiInfo = new WifiInfo();
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
                        }
                    }

                    wifiInfo.DefualtGateway = DefualtGatewayAddress.ToString();
                    
                    try
                    {

                        UnicastIPAddressInformationCollection UnicastIPInfoCol = Interface.GetIPProperties().UnicastAddresses;
                        foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol)
                        {
                            if (UnicatIPInfo.Address.IsIPv6LinkLocal)
                            {

                                wifiInfo.IPv6 = UnicatIPInfo.Address.ToString();


                            }
                            else
                            {
                                wifiInfo.IPv4 = UnicatIPInfo.Address.ToString();
                            }
                        }
                        wifiInfo.IPv6 = String.IsNullOrEmpty(wifiInfo.IPv6) ? "N/A" : wifiInfo.IPv6;
                        wifiInfo.DefualtGatewayMac = GetMacAddress(DefualtGatewayAddress);
                        wifiInfo.Speed = (Interface.Speed / 1000000) + " Mbps ";
                        wifiInfo.RSSID = wlanIface.RSSI.ToString();

                       
                    }
                    catch (Exception ex )
                    {
                      
                      // continue;
                    }

                    
                    wifiInfo.Channel = wlanIface.Channel.ToString();
                    byte[] macAddr = WlanConnection.wlanAssociationAttributes.dot11Bssid;
                    string tMacBSSID = "";
                    for (int i = 0; i < macAddr.Length; i++)
                    {

                        tMacBSSID += macAddr[i].ToString("x2").PadLeft(2, '0').ToUpper() + ":";

                    }
                    tMacBSSID = tMacBSSID.Substring(0, tMacBSSID.Length - 1);
                    wifiInfo.SSID = CleanInput(System.Text.ASCIIEncoding.ASCII.GetString(WlanConnection.wlanAssociationAttributes.dot11Ssid.SSID).ToString());
                    wifiInfo.Signal = WlanConnection.wlanAssociationAttributes.wlanSignalQuality.ToString() + "%";
                    wifiInfo.BSS = tMacBSSID;
                    string sMac = string.Join(":", (from z in wlanIface.NetworkInterface.GetPhysicalAddress()
                             .GetAddressBytes()
                                                    select z.ToString("X2")).ToArray());
                    wifiInfo.MAC = sMac;
                    wifiInfo.Security = WlanConnection.wlanSecurityAttributes.dot11AuthAlgorithm.ToString() + "  " + WlanConnection.wlanSecurityAttributes.dot11CipherAlgorithm.ToString();
                    if (wifiInfo.BSS != null)
                    {
                        wifiInfo.Vendor = PropertyProvider.getPropertyProvider().getVendorByMacAddress(wifiInfo.BSS.Substring(0, 8).Replace(":", ""));

                    }
                    else
                    {

                        wifiInfo.Vendor = "N/A";
                    }

                    wifiConeectedInfoList.Add(wifiInfo);
                }
            }
            catch (Exception ex)
            {
            }

            wifiConeectedInfoList.AddRange(getNotConnectWifiList());

            /// overlapping aps
            /// 
            int countAPs = 0;
            List<int> removeIndex = new List<int>();
            foreach (var item in wifiConeectedInfoList)
            {
                if (item.DefualtGateway != null)
                {
                    int index = 0;
                    foreach (var item2 in wifiConeectedInfoList)
                    {
                        
                        if (item.SSID == item2.SSID && item2.DefualtGateway == null)
                        {
                            
                            removeIndex.Add(index);
                         }
                        index++;

                    }
                    
                }
            }

            foreach (var item in removeIndex)
            {
                wifiConeectedInfoList.RemoveAt(item);
            }

            wifiConeectedInfoList = wifiConeectedInfoList.Distinct(new DistinctItemComparer()).ToList();
            foreach (var item in wifiConeectedInfoList)
            {
                if (item.DefualtGateway != null)
                {
                    //String ChannelConeccted = "";
                    foreach (var item2 in wifiConeectedInfoList)
                    {
                        
                            if (item.Channel == item2.Channel && item.SSID != item2.SSID)
                            {
                                countAPs++;
                            }
                         

                    }
                    item.OverlappingAPS = countAPs.ToString();


                }
            }
            
             
            return leastCongestion(wifiConeectedInfoList);

        }

        //not connected wifi info

        List<WifiInfo> getNotConnectWifiList()
        {
            List<WifiInfo> wifiNotConnectedInfoList = new List<WifiInfo>();

            WlanClient client = new WlanClient();
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                 WifiInfo wifiInfo = new WifiInfo();
                Wlan.WlanAvailableNetwork[] networks = null;
                try
                {
                    networks = wlanIface.GetAvailableNetworkList(0);
                }
                catch (Exception ex)
                {

                    continue;
                }
               
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {
                    try
                    {
                        WifiInfo NWifiInfo1 = new WifiInfo();
                        NWifiInfo1.SSID = CleanInput(System.Text.ASCIIEncoding.ASCII.GetString(network.dot11Ssid.SSID).ToString());
                        //if (wifiInfo.SSID == NWifiInfo1.SSID)
                        // continue;
                        NWifiInfo1.Signal = network.wlanSignalQuality + "%";
                        NWifiInfo1.Security = network.dot11DefaultAuthAlgorithm.ToString() + "  " + network.dot11DefaultCipherAlgorithm.ToString();

                        wifiNotConnectedInfoList.Add(NWifiInfo1);

                    }
                    catch (Exception)
                    {

                        throw;
                    }


                }


                Wlan.WlanBssEntry[] wlanBssEntries = wlanIface.GetNetworkBssList();

                foreach (Wlan.WlanBssEntry network in wlanBssEntries)
                {
                    int rss = network.rssi;
                    foreach (var NWifiInfo in wifiNotConnectedInfoList)
                    {
                        if (NWifiInfo.SSID == CleanInput(System.Text.ASCIIEncoding.ASCII.GetString(network.dot11Ssid.SSID).ToString()))
                        {
                            NWifiInfo.BSS = getMacFromBytes(network.dot11Bssid);
                            NWifiInfo.RSSID = network.rssi.ToString();
                            NWifiInfo.Channel = ConvertToChannel(network.chCenterFrequency).ToString();
                            if (NWifiInfo.BSS != null)
                            {
                                NWifiInfo.Vendor = PropertyProvider.getPropertyProvider().getVendorByMacAddress(NWifiInfo.BSS.Substring(0, 8).Replace(":", ""));

                            }
                            else
                            {

                                NWifiInfo.Vendor = "NA";
                            }

                        }
                    }



                }
            }


            return wifiNotConnectedInfoList;
        }
        List<WifiInfo> leastCongestion(List<WifiInfo> wifiInfoList)
        {

            int[] PTA_APPROVED_CHANNELS_5_GHZ = new int[] { 149, 153, 157, 161, 165, 169, 173, 177 };
            List<WifiInfo> List_LC_5_GHZ = new List<WifiInfo>();
            foreach (var item in wifiInfoList)
            {
                int signal = int.Parse(item.Signal.Replace("%", ""));
                int channel = int.Parse(item.Channel);
                if (PTA_APPROVED_CHANNELS_5_GHZ.Contains(channel))
                {

                    List_LC_5_GHZ.Add(item);
                }

            }


            String _5_GHZ = "";
            if (List_LC_5_GHZ.Count > 0)
            {

                var groupedCustomerList_5_GHZ = List_LC_5_GHZ
   .GroupBy(u => u.Channel).OrderByDescending(u => u.Count())
   .Select(grp => grp.ToList())
   .ToList().GroupBy(u => u.Count()).OrderByDescending(u => u.Count())
    .Select(grp => grp.ToList())
    .ToList().First().GroupBy(x => x.Max(e => e.Signal.Replace("%", "")))
            .Select(grp => grp.ToList())
            .ToList().First();
                _5_GHZ = groupedCustomerList_5_GHZ.First().First().Channel;
            }
            else
            {

                _5_GHZ = PTA_APPROVED_CHANNELS_5_GHZ[0].ToString();
            }

            if (List_LC_5_GHZ.Count() < PTA_APPROVED_CHANNELS_5_GHZ.Count())
            {
                foreach (var chanel in PTA_APPROVED_CHANNELS_5_GHZ)
                {
                    bool isFind = false;
                    foreach (var item in List_LC_5_GHZ)
                    {
                        if (chanel.ToString() == item.Channel)
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        _5_GHZ = chanel.ToString();
                        break;
                    }
                }
            }

            //var frequency = channel_4_GHZ.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            //foreach (var grp in channel_4_GHZ.GroupBy(i => i))
            // {
            //     groupedChannel_4_GHZ.Add(grp.Key, grp.Count());
            // }
            int LeastChannel = 0;
            try
            {
                LeastChannel = leastCongestionOverLappingOn_2_4(wifiInfoList);

            }
            catch (Exception ex)
            {

                throw;
            }
            foreach (var item in wifiInfoList)
            {
                item.GHZ5 = _5_GHZ;
                item.GHZ4 = LeastChannel.ToString();
            }

            return wifiInfoList;
        }
        int leastCongestionOverLappingOn_2_4(List<WifiInfo> wifiInfoList)
        {
            int[] PTA_APPROVED_CHANNELS_4_GHZ = new int[] { 1, 6, 11 };
            int[] OVERLAP_CHANNELS_4_GHZ_FIRST = new int[] { 2, 3, 4, 5 };
            int[] OVERLAP_CHANNELS_4_GHZ_SECOND = new int[] { 7, 8, 9, 10 };
            int[] OVERLAP_CHANNELS_4_GHZ_THIRD = new int[] { 12, 13 };

            int ChCountONE = 0;
            int ChCountSIX = 0;
            int ChCountELEVEN = 0;

            int MaxChannelSignal = 0;
            int MaxSignalPre = 0;

            String strChannel = "";

            foreach (var item in wifiInfoList)
            {
                int signal = int.Parse(item.Signal.Replace("%", ""));
                int channel = int.Parse(item.Channel);
                strChannel += "," + channel;




                if (OVERLAP_CHANNELS_4_GHZ_FIRST.Contains(channel))
                {
                    ChCountONE++;
                    ChCountSIX++;
                }
                else if (OVERLAP_CHANNELS_4_GHZ_SECOND.Contains(channel))
                {
                    ChCountSIX++;
                    ChCountELEVEN++;
                }
                else if (OVERLAP_CHANNELS_4_GHZ_THIRD.Contains(channel))
                {
                    ChCountELEVEN++;
                }
                else
                {

                    if (PTA_APPROVED_CHANNELS_4_GHZ[0] == channel)
                    {
                        ChCountONE++;
                        //Max Channel Signal record
                        if (signal > MaxSignalPre)
                        {
                            MaxSignalPre = signal;
                            MaxChannelSignal = channel;
                        }
                    }

                    if (PTA_APPROVED_CHANNELS_4_GHZ[1] == channel)
                    {
                        ChCountSIX++;
                        //Max Channel Signal record
                        if (signal > MaxSignalPre)
                        {
                            MaxSignalPre = signal;
                            MaxChannelSignal = channel;
                        }
                    }
                    if (PTA_APPROVED_CHANNELS_4_GHZ[2] == channel)
                    {
                        ChCountELEVEN++;
                        //Max Channel Signal record
                        if (signal > MaxSignalPre)
                        {
                            MaxSignalPre = signal;
                            MaxChannelSignal = channel;
                        }

                    }
                }
            }
            //find most least congested channel

            Dictionary<int, int> Condic = new Dictionary<int, int>();
            Condic.Add(PTA_APPROVED_CHANNELS_4_GHZ[0], ChCountONE);
            Condic.Add(PTA_APPROVED_CHANNELS_4_GHZ[1], ChCountSIX);
            Condic.Add(PTA_APPROVED_CHANNELS_4_GHZ[2], ChCountELEVEN);

            double max = Condic.Min(kvp => kvp.Value);

            var minPair = Condic.Where(kvp => kvp.Value == max);
            int SugestionChanel = 0;
            if (minPair.Count() > 1)
            {
                Dictionary<int, int> MaxSignaldic = new Dictionary<int, int>();

                foreach (var item in minPair)
                {
                    var maxSignal = wifiInfoList
  .Where(u => (item.Key.ToString() == u.Channel)).Max(e => e.Signal.Replace("%", ""));
                    if (maxSignal != null)
                    {
                        MaxSignaldic.Add(item.Key, int.Parse(maxSignal));
                    }
                     
                }
                if (MaxSignaldic.Count > 0)
                {
                    double maxChhanel = MaxSignaldic.Min(kvp => kvp.Value);
                    var resultChannel = MaxSignaldic.Where(kvp => kvp.Value == maxChhanel);
                    SugestionChanel = resultChannel.First().Key;
                }
                else {
                    SugestionChanel = minPair.First().Key;

                }


            }
            else
            {

                SugestionChanel = minPair.First().Key;
            }










            return SugestionChanel;
        }
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(uint destIP, uint srcIP, byte[] macAddress, ref uint macAddressLength);
        public String getMacFromBytes(byte[] nMacAddr)
        {
            string NTMacBSSID = "";
            for (int i = 0; i < nMacAddr.Length; i++)
            {

                NTMacBSSID += nMacAddr[i].ToString("x2").PadLeft(2, '0').ToUpper() + ":";

            }
            return NTMacBSSID.Substring(0, NTMacBSSID.Length - 1);
        }

        public String GetMacAddress(IPAddress address)
        {
            byte[] mac = new byte[6];
            uint len = (uint)mac.Length;
            byte[] addressBytes = address.GetAddressBytes();
            uint dest = ((uint)addressBytes[3] << 24)
              + ((uint)addressBytes[2] << 16)
              + ((uint)addressBytes[1] << 8)
              + ((uint)addressBytes[0]);
            if (SendARP(dest, 0, mac, ref len) != 0)
            {
                throw new Exception("The ARP request failed.");
            }
            byte[] macAddr = mac;
            string tMac = "";
            for (int i = 0; i < macAddr.Length; i++)
            {

                tMac += macAddr[i].ToString("x2").PadLeft(2, '0').ToUpper() + ":";

            }
            tMac = tMac.Substring(0, tMac.Length - 1);
            return tMac;
        }
        public Task<IEnumerable<WifiInfo>> GetWifiesAsync()
        {
            return Task.FromResult(getWifiList());
        }

        string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters, 
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }
        /// <summary>
        /// Converts a frequency value to a designated WIFI channel
        /// </summary>
        /// <param name="frequency">frequency to convert to a channel</param>
        /// <returns>the WIFI channel at the given frequency</returns>
        public static uint ConvertToChannel(uint frequency)
        {
            uint retVal = 0;
            // 2.4 GHz
            if ((frequency > 2400000) && (frequency < 2484000))
                retVal = (frequency - 2407000) / 5000;

            if ((frequency >= 2484000) && (frequency <= 2495000))
                retVal = 14;

            // 5 GHz
            if ((frequency > 5000000) && (frequency < 5900000))
                retVal = (frequency - 5000000) / 5000;

            return retVal;
        }
        public IPAddress GetDefaultGateway()
        {
            var gateway_address = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(e => e.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(e => e.GetIPProperties().GatewayAddresses)
                    .FirstOrDefault();
            if (gateway_address == null) return null;
            return gateway_address.Address;
        }
    }



    class DistinctItemComparer : IEqualityComparer<WifiInfo>
    {

        public bool Equals(WifiInfo x, WifiInfo y)
        {
            return x.SSID == y.SSID &&
                x.BSS == y.BSS;
               
        }

        public int GetHashCode(WifiInfo obj)
        {
            return obj.SSID.GetHashCode() ^
                obj.BSS.GetHashCode()  
                ;
        }
    }
}
