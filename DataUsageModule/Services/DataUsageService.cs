using System;

using System.Threading.Tasks;
using DataUsageModule.Model;
using System.Net.NetworkInformation;
using System.ComponentModel.Composition;
using ViewSwitchingNavigation.Infrastructure.Utils;
using System.Diagnostics;

namespace DataUsageModule.Services
{
    [Export(typeof(IDataUsageService))]
   public class DataUsageService : IDataUsageService
    {
        long BytesReceivedPrev = 0;
        long BytesSentPrev = 0;
        Stopwatch sw;

        public DataUsage  GetDataUsageAsync()
        {
            return   GetDataUsage() ;
        }

        public void  clearDataUsage()
        {
            BytesReceivedPrev = 0;
            BytesSentPrev = 0;
            if (sw != null) {
                sw.Reset();
            }
        }

        private DataUsage GetDataUsage()
        {

            if (!NetworkInterface.GetIsNetworkAvailable())
                return null;

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
             DataUsage DataUsage = new DataUsage();
            long bytesReceived = 0;
            long bytesSent = 0;

            foreach (NetworkInterface inf in interfaces)
            {
                if (inf.OperationalStatus == OperationalStatus.Up &&
                    inf.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    inf.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    inf.NetworkInterfaceType != NetworkInterfaceType.Unknown && !inf.IsReceiveOnly)
                {
                    bytesReceived += inf.GetIPv4Statistics().BytesReceived;
                    bytesSent      += inf.GetIPv4Statistics().BytesSent;
                     
                
                }
            }

            if (BytesReceivedPrev == 0)
            {
                sw = new Stopwatch();
                sw.Start();
                BytesReceivedPrev = bytesReceived;
                BytesSentPrev = bytesSent;
                DataUsage.Upload = UtilConvert.SizeToSpeed((bytesSent));
                DataUsage.Download = UtilConvert.SizeToSpeed((bytesReceived));


            }
            else {
                DataUsage.Upload = UtilConvert.SizeToSpeed((long)((bytesSent - BytesSentPrev)/sw.Elapsed.TotalSeconds));
                DataUsage.Download = UtilConvert.SizeToSpeed((long)((bytesReceived - BytesReceivedPrev)/ sw.Elapsed.TotalSeconds));
            }


            BytesReceivedPrev = bytesReceived;
            BytesSentPrev = bytesSent;
            return DataUsage;
        }
    }
}
