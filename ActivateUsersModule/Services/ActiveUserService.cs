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

namespace ActivateUsersModule.Services
{
    [Export(typeof(IActiveUserService))]

    class ActiveUserService : IActiveUserService
    {
        Dictionary<String,Ping> DicPingList;
        List<String> activeuser;
        List<String> notactiveuser;
        ActiveUserOnNetwork NetworkUser;
        userResult _userResutl;
        bool isStop = false;
        private IEnumerable<ActiveUser> getActiveUserList()
        {

            List<ActiveUser> activeUserList = new List<ActiveUser>();

            ActiveUserOnNetwork NetworkUser = new ActiveUserOnNetwork();

            Dictionary<IPAddress, PhysicalAddress> all = NetworkUser.GetAllDevicesOnLAN();
 
            foreach (KeyValuePair<IPAddress, PhysicalAddress> kvp in all)
            {
                ActiveUser user = new ActiveUser();

                user.Ip = (kvp.Key != null) ? kvp.Key.ToString():"";
                user.Mac = (kvp.Value != null) ? kvp.Value.ToString() : "";
                activeUserList.Add(user);
                //  kvp.Key, kvp.Value



            }

            return activeUserList;
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

        public void getUsers(Double timeOut,userResult user)
        {
            _userResutl = user;
            DicPingList = new Dictionary<string, Ping>();
            activeuser = new List<String>();
            notactiveuser = new List<String>();
            NetworkUser = new ActiveUserOnNetwork();
            isStop = false;


            IPAddress LocalIP = IPAddress.Parse(UtilNetwork.GetLocalIPAddress());
            IPAddress subnetmask = UtilNetwork.GetSubnetMask(IPAddress.Parse(UtilNetwork.GetLocalIPAddress()));

            //IPAddress NetworkAddres =  IPAddressExtensions.GetNetworkAddress(subnetmask, IPAddress.Parse(UtilNetwork.GetLocalIPAddress()));
            IPAddress NetworkAddres = LocalIP.GetNetworkAddress(subnetmask);
            IPAddress BroadcastAdress = LocalIP.GetBroadcastAddress(subnetmask);
            IEnumerable<IPAddress> address = GetIPRange(NetworkAddres, BroadcastAdress);
            

            System.Timers.Timer upTimer = new System.Timers.Timer(timeOut);
            // Hook up the Elapsed event for the timer. 
 
            // AutoResetEvent waiter = new AutoResetEvent(false);
            upTimer.Elapsed += (s, e) =>
            {
                isStop = true;
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
                if (_userResutl != null)
                    _userResutl(null);
                _userResutl = null;

            };
            upTimer.AutoReset = false;
            upTimer.Enabled = true;



            foreach (IPAddress ipAddress in address)
            {
                if (isStop)
                    break;
                  Ping p = new Ping();
                  DicPingList.Add(ipAddress.ToString(),p); 
            }
            if (isStop)
                return;
            Dictionary<string, Ping> Dic = new Dictionary<string, Ping>(DicPingList);

 
            foreach (var item in Dic)
            {
                Thread.Sleep(100);
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
            
             





            // });
            //     t1.Start();




        }

        private   void PingCompletedCallback(object sender, PingCompletedEventArgs e)
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
               // activeuser.Add(IP);

              

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
            if (DicPingList.Count() == 1) {

            }
                if (DicPingList.Count() == 1)
            {
                if (!isStop)
                {
                    if (_userResutl != null)
                        _userResutl(null);
                    _userResutl = null;

                }

            }



        }

        public void  FindMAC(string IP)
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
            backgroundWorker.RunWorkerCompleted += worker_RunWorkerCompleted;   
            
            backgroundWorker.RunWorkerAsync(IP);
 
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
             ActiveUser user= (ActiveUser)e.Result;
             if (_userResutl != null)
                 _userResutl(user);
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
                if(!String.IsNullOrEmpty(mac))
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

        public void stopTest()
        {

            isStop = true;
            if (_userResutl != null)
                _userResutl(null);
            _userResutl = null;
            if (DicPingList != null)
                if (DicPingList.Count() > 1)
                {
                    Dictionary<string, Ping> Dic1 = new Dictionary<string, Ping>(DicPingList);
                    DicPingList.Clear();
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
