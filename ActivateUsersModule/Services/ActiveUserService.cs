using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActivateUsersModule.Model;
using System.Net.NetworkInformation;
using System.Net;
using CoreLibrary;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.ComponentModel;
using ViewSwitchingNavigation.Infrastructure.Utils;
using System.Threading;
using ViewSwitchingNavigation.Infrastructure;
using ViewSwitchingNavigation.Infrastructure.Log;
using Prism.Logging;

namespace ActivateUsersModule.Services
{
    [Export(typeof(IActiveUserService))]

    class ActiveUserService : IActiveUserService
    {
        Dictionary<String, Ping> DicPingList;
        List<String> ActiveUserIpList;
        List<String> ActiveUserRequestForHostIpList;
        Dictionary<IPAddress, PhysicalAddress> withoutHostUser;
        ActiveUserOnNetwork NetworkUser;
        userResult _userResutl;
        int CountRunningThread = 0;
        int CountThreadCompeleted = 0;
        int ActiveUserCount = 0;
        IPAddress NetworkAddres;
        IPAddress BroadcastAdress;
        //IPAddress RoterAdress = IPAddress.Parse("192.168.0.1");
        
        List<ActiveUser> activeUserList = new List<ActiveUser>();
        bool isStop = false;
        List<BackgroundWorker> BackgroundWorkerList = new List<BackgroundWorker>();
     static   List<Thread> HostBackgroundWorkerList = new List<Thread>();
        System.Timers.Timer upTimer;
        System.Timers.Timer reTimer;

        
        private IEnumerable<ActiveUser> getActiveUserList()
        {

            List<ActiveUser> activeUserList1 = new List<ActiveUser>();

            ActiveUserOnNetwork NetworkUser = new ActiveUserOnNetwork();

            Dictionary<IPAddress, PhysicalAddress> all = NetworkUser.GetAllDevicesOnLAN();
            //all.Remove(NetworkAddres);
            //all.Remove(BroadcastAdress);
           
            foreach (KeyValuePair<IPAddress, PhysicalAddress> kvp in all)
            {
                ActiveUser user = new ActiveUser();

                user.Ip = (kvp.Key != null) ? kvp.Key.ToString() : "";
                user.Mac = (kvp.Value != null) ? kvp.Value.ToString() : "";
                activeUserList1.Add(user);
                //  kvp.Key, kvp.Value



            }

            return activeUserList1;
        }


        public Task<IEnumerable<ActiveUser>> getActiveUsersAsync()
        {
            return Task.FromResult(getActiveUserList());
        }





        public static IEnumerable<IPAddress> GetIPRange(IPAddress startIP, IPAddress endIP)
        {
            uint sIP = IPAddressToUInt(startIP.GetAddressBytes());
            uint eIP = IPAddressToUInt(endIP.GetAddressBytes());
            while (sIP <= eIP)
            {
                yield return new IPAddress(ReverseBytesArray(sIP));
                sIP++;
            }
        }

        public static uint ReverseBytesArray(uint ip)
        {
            byte[] bytes = BitConverter.GetBytes(ip);
            bytes = bytes.Reverse().ToArray();
            return (uint)BitConverter.ToInt32(bytes, 0);
        }

        public static uint IPAddressToUInt(byte[] ipBytes)
        {
            ByteConverter bConvert = new ByteConverter();
            uint ipUint = 0;

            int shift = 24;
            foreach (byte b in ipBytes)
            {
                if (ipUint == 0)
                {
                    ipUint = (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                    shift -= 8;
                    continue;
                }

                if (shift >= 8)
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                else
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint));

                shift -= 8;
            }

            return ipUint;
        }

        public   void getUsers(Double timeOut, userResult user)
        {
            _userResutl = user;
            DicPingList = new Dictionary<string, Ping>();
            ActiveUserIpList = new List<String>();
            ActiveUserRequestForHostIpList = new List<String>();
            NetworkUser = new ActiveUserOnNetwork();
            Dictionary<IPAddress, PhysicalAddress> withoutHostUser = new Dictionary<IPAddress, PhysicalAddress>() ;
            isStop = false;
            activeUserList = new List<ActiveUser>();
            ActiveUserCount = 0;
            CountRunningThread = 0;
            CountThreadCompeleted = 0;

            IPAddress LocalIP = IPAddress.Parse(UtilNetwork.GetLocalIPAddress());
            IPAddress subnetmask = UtilNetwork.GetSubnetMask(IPAddress.Parse(UtilNetwork.GetLocalIPAddress()));

            //IPAddress NetworkAddres =  IPAddressExtensions.GetNetworkAddress(subnetmask, IPAddress.Parse(UtilNetwork.GetLocalIPAddress()));
              NetworkAddres = LocalIP.GetNetworkAddress(subnetmask);
              BroadcastAdress = LocalIP.GetBroadcastAddress(subnetmask);
            IEnumerable<IPAddress> address = GetIPRange(NetworkAddres, BroadcastAdress);


             upTimer = new System.Timers.Timer(timeOut - 5000);
            // Hook up the Elapsed event for the timer. 

            // AutoResetEvent waiter = new AutoResetEvent(false);
            upTimer.Elapsed += (s, e) =>
            {
                retryArpUser(5000);
            };
            upTimer.AutoReset = false;
            upTimer.Enabled = true;



            Dictionary<IPAddress, PhysicalAddress> all = NetworkUser.GetAllDevicesOnLAN();     
           // all.Remove(NetworkAddres);
            //all.Remove(BroadcastAdress);

            int count = 0;
            foreach (IPAddress ipAddress in address)
            {
                if (isStop)
                    break;
                if (ipAddress.Equals(NetworkAddres) || ipAddress.Equals(BroadcastAdress) )
                {

                 }
                    
                if (all.ContainsKey(ipAddress))
                {
                    ActiveUser activeuser = new ActiveUser();
                    activeuser.Ip = ipAddress.ToString();
                    activeuser.Mac = all[ipAddress] != null ? all[ipAddress].ToString(): "N/A";
                    if (!String.IsNullOrEmpty(activeuser.Mac)  && activeuser.Mac != "N/A")
                    {
                        activeuser.Mac = UtilNetwork.GetMacAddress(activeuser.Mac);
                        activeuser.Vendor = PropertyProvider.getPropertyProvider().getVendorByMacAddress(activeuser.Mac.Substring(0, 8).Replace(":", ""));

                    }
                    //arpForHostList.Add(ipAddress);
                    // ActiveUserIpList.Add(ipAddress.ToString());
                    activeUserList.Add(activeuser);
                    withoutHostUser.Add(ipAddress, all[ipAddress]);
                    ActiveUserCount++;

                        count++;
                        FindDeviceNameThread(activeuser);
                   
               
                   
                    continue;
                }

                Ping p = new Ping();
                DicPingList.Add(ipAddress.ToString(), p);
            }
            if (isStop)
                return;




            Dictionary<string, Ping> Dic = new Dictionary<string, Ping>(DicPingList);


            foreach (var item in Dic)
            {
                if (isStop)
                    break;
                Ping p = item.Value;
                try
                {
                    p.SendAsync(item.Key, 5000, item.Key);
                    p.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);

                }
                catch (Exception ex)
                {

                    break;

                }

                if (isStop)
                {
                    break;
                }
            }











        }
        void retryArpUser(int timeout)
        {
            reTimer = new System.Timers.Timer(timeout);
            // Hook up the Elapsed event for the timer. 

            // AutoResetEvent waiter = new AutoResetEvent(false);
            upTimer.Elapsed += (s, e) =>
            {

                stopTest();
            };
            upTimer.AutoReset = false;
            upTimer.Enabled = true;
            Dictionary<IPAddress, PhysicalAddress> all = NetworkUser.GetAllDevicesOnLAN();
             all.Remove(NetworkAddres);
            all.Remove(BroadcastAdress);

            foreach (var item in all)
            {
                if (!ActiveUserRequestForHostIpList.Contains(item.Key.ToString()))
                {
                    ActiveUser activeuser = new ActiveUser();
                    activeuser.Ip = item.Key.ToString();
                    activeuser.Mac = item.Value != null ? all[item.Key].ToString()  : "N/A";
                    if (!String.IsNullOrEmpty(activeuser.Mac) && activeuser.Mac != "N/A")
                    {
                        activeuser.Mac = UtilNetwork.GetMacAddress(activeuser.Mac);
                        activeuser.Vendor = PropertyProvider.getPropertyProvider().getVendorByMacAddress(activeuser.Mac.Substring(0, 8).Replace(":", ""));

                    }
                        activeUserList.Add(activeuser);
                    
                        FindDeviceNameThread(activeuser);
                        ActiveUserCount++;
                }
            }

        }
        void FindDeviceNameThread(ActiveUser user)
        {
        // CustomLogger.PrintLog("FindDeviceNameThread:" + user.Ip, Category.Debug, Priority.Low);

            if (!ActiveUserRequestForHostIpList.Contains(user.Ip))
            {
                ActiveUserRequestForHostIpList.Add(user.Ip);
            }
            else
            {
                return;
            }


            Thread thread = null;
            
            thread = new Thread(new ThreadStart(() => {

                //if thread interuped
                try {
                    String IP = user.Ip;
                    String mac = user.Mac;
                    string machineName;
                try
                {
                       
                    IPHostEntry hostEntry = Dns.GetHostEntry(IP);
                    machineName = hostEntry.HostName;
                   //CustomLogger.PrintLog("HostName:" + hostEntry.HostName + " hostEntry:" + hostEntry.AddressList + " hostEntry.Ip:" + hostEntry.AddressList.First().ToString() + " user.Ip:" + user.Ip, Category.Debug, Priority.Low);

                       // if (hostEntry.AddressList.Count() > 1) {
                         
                       //     machineName = "UNKNOWN";
                       // }
 




                }
                catch (Exception ex)
                {
                   //CustomLogger.PrintLog(ex.Message, Category.Debug,Priority.None);
                    machineName = "UNKNOWN";
                }
                    if (isStop) {
                        HostBackgroundWorkerList.Remove(thread);
                        return;

                    }

                    activeUserFound(IP, mac, machineName, user.Vendor);
                CountThreadCompeleted++;
               // CustomLogger.PrintLog("Totoal Active User:"+ ActiveUserCount +"Total Completed:" + CountThreadCompeleted, Category.Debug, Priority.Low);

                if (CountThreadCompeleted == ActiveUserCount)
                {
                        CustomLogger.PrintLog("Thread Completed: Totoal Active User:" + ActiveUserCount + "Total Completed:" + CountThreadCompeleted, Category.Debug, Priority.Low);

                        stopTest();
                }
                if (CountThreadCompeleted > 100)
                {

                }
                try
                {
                   HostBackgroundWorkerList.Remove(thread);

                }
                catch (Exception ex)
                {
                    CustomLogger.PrintLog(ex.Message, Category.Exception, Priority.High);

                }



                }
                catch (Exception ex)
                {
                    CustomLogger.PrintLog("Thread Id :"+thread.ManagedThreadId+" "+ ex.Message, Category.Exception, Priority.Low);
                    HostBackgroundWorkerList.Remove(thread);

                }



            }));
            thread.Start();
            HostBackgroundWorkerList.Add(thread);
            //CustomLogger.PrintLog("Total Thread:"+ HostBackgroundWorkerList.Count, Category.Debug, Priority.Low);




        }

        void activeUserFound(String IpAddress, String Mac, String machineName,String vendor)
        {
            //if (IpAddress == NetworkAddres.ToString() || IpAddress == BroadcastAdress.ToString() || IpAddress == RoterAdress.ToString())
            //{
            //    return;
            //}
            ActiveUser user = new ActiveUser();
            user.Ip = IpAddress;
            user.Mac = Mac;
            user.HostName = machineName;
            user.Vendor = vendor;
            if (_userResutl != null)
            {
                _userResutl(user, false);
                ActiveUserIpList.Remove(IpAddress);

            }

            if (CountRunningThread == 0 || CountRunningThread == 1)
            {

            }

            activeUserList.Remove(user);

        }

        private void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            // If the operation was canceled, display a message to the user.
            try
            {
                DicPingList.Remove((String)e.UserState);

            }
            catch (Exception)
            {


            }
            if (isStop)
            {

                return;
            }
            if (e.Cancelled)
            {

                // Let the main thread resume. 
                // UserToken is the AutoResetEvent object that the main thread 
                // is waiting for.
            }

            // If an error occurred, display the exception to the user.
            if (e.Error != null)
            {


                // Let the main thread resume. 
                //  ((AutoResetEvent)e.UserState).Set();
            }

            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {



                String IP = (e.Reply.Address).ToString();
                ActiveUserIpList.Add(IP);
                // activeuser.Add(IP);
                ActiveUserCount++;

                FindMAC(IP);

                //  String deviceName = await UtilNetwork.GetMachineNameFromIPAddress(IP);

                //String  deviceName = NetworkUser.GetMachineNameFromIPAddress(IP);
                // UtilNetwork.DoGetHostEntryAsync(IP).;//.GetMachineNameFromIPAddress(IP);
                //activeUser.HostName = deviceName;

            }
            else
            {
                // notactiveuser.Add((e.Reply.Address).ToString());

            }


            if (DicPingList.Count == 0 || DicPingList.Count == 1)
            {

            }

        }

        public void FindMAC(string IP)
        {
            
            BackgroundWorker backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true

            };

            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.WorkerReportsProgress = true;

            backgroundWorker.ProgressChanged += worker_ProgressChanged;
            backgroundWorker.DoWork += worker_DoWork;
            backgroundWorker.RunWorkerCompleted += mac_worker_RunWorkerCompleted;

            backgroundWorker.RunWorkerAsync(IP);
            BackgroundWorkerList.Add(backgroundWorker);
            
        }

        void mac_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (isStop) {
                return;
            }
            BackgroundWorkerList.Remove(sender as BackgroundWorker);    
            ActiveUser user = (ActiveUser)e.Result;
            FindDeviceNameThread(user);
            activeUserList.Add(user);
            CountRunningThread++;
         }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            String IP = (String)e.Argument;   // the 'argument' parameter resurfaces here
            ActiveUser activeUser = new ActiveUser();
            activeUser.Ip = IP;
            if (IP != null)
            {
                String mac = GetMacAddress(IP); ;//NetworkUser.getMAC(IP);
                activeUser.Mac = mac;
                activeUser.Vendor = "";
                if (!String.IsNullOrEmpty(mac))
                {
                    activeUser.Vendor = PropertyProvider.getPropertyProvider().getVendorByMacAddress(mac.Substring(0, 8).Replace(":", ""));

                }

                String HostName;
                try
                {
                    // Retrieve the "Host Name" for this IP Address. This is the "Name" of the machine.
                    //var hostEntry = await Dns.GetHostEntryAsync(IP);
                    //HostName = hostEntry.HostName;

                    // HostName =   Dns.GetHostEntry(IP).HostName;

                }
                catch
                {
                    //   HostName = string.Empty;
                }
                //activeUser.HostName = HostName;

            }
            else
            {
                activeUser.Mac = "";

            }

            e.Result = activeUser;
        }
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // pbCalculationProgress.Value = e.ProgressPercentage;


        }


        void disposeAllThreads()
        {
            if (BackgroundWorkerList == null)
                return;
            List<BackgroundWorker> aBackgroundWorker = new List<BackgroundWorker>(BackgroundWorkerList);

            foreach (var item in aBackgroundWorker)
            {
                if(item != null)
                item.CancelAsync();
            }

            List<Thread> HoadtBackgroundWorker = new List<Thread>(HostBackgroundWorkerList);

             foreach (var item in HoadtBackgroundWorker)
            {
                if (item != null) {
                     //item.Interrupt();
                    item.Abort();
                     CustomLogger.PrintLog("Interrupt:" + item.ManagedThreadId , Category.Debug, Priority.Low);
                }
            }
            HostBackgroundWorkerList.Clear();
           aBackgroundWorker.Clear();
            BackgroundWorkerList.Clear();
        }
        void disposeAllPingObject() {
            if (DicPingList == null)
                return;
            Dictionary<string, Ping> Dic1 = new Dictionary<string, Ping>(DicPingList);
            for (int i = 0; i < Dic1.Count; i++)
            {
               

                Ping p = Dic1.ElementAt(i).Value;
                if (p != null)
                {
                    p.SendAsyncCancel();
                    p.Dispose();
                }

            }
        }
        void updateLastResult() {
            if (activeUserList == null)
                return;
            List<ActiveUser> activeList = new List<ActiveUser>(activeUserList);
            foreach (var item in activeList)
            {
               
                activeUserFound(item.Ip, item.Mac, "N/A", item.Vendor);

            }
            if (ActiveUserIpList == null)
                return;
            List<String> ListPing = new List<String>(ActiveUserIpList);

            foreach (var item in ListPing)
            {
                activeUserFound(item, "N/A", "N/A", "N/A");
            }
        }
        public void stopTest()
        {
            if (upTimer != null) {
                upTimer.Stop();
                upTimer.Dispose();
            }
            if (reTimer != null) {
                reTimer.Stop();
                reTimer.Dispose();

            }
            
            isStop = true;
            disposeAllPingObject();
            updateLastResult();
            if (_userResutl != null)
                _userResutl(null, true);
            _userResutl = null;
            disposeAllThreads();
        }
        public string GetHostName(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {

                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + ":" + substrings[4].ToUpper() + ":" + substrings[5].ToUpper() + ":" + substrings[6].ToUpper()
                         + ":" + substrings[7].ToUpper() + ":"
                         + substrings[8].Substring(0, 2).ToUpper();
                return macAddress.ToUpper();
            }

            else
            {
                return "";
            }
        }
        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {

                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + ":" + substrings[4].ToUpper() + ":" + substrings[5].ToUpper() + ":" + substrings[6].ToUpper()
                         + ":" + substrings[7].ToUpper() + ":"
                         + substrings[8].Substring(0, 2).ToUpper();
                return macAddress.ToUpper();
            }

            else
            {
                return "";
            }
        }
    }
}
