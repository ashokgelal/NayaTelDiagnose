using System;

using System.Threading.Tasks;
using DataUsageModule.Model;
using System.Net.NetworkInformation;
using System.ComponentModel.Composition;
using ViewSwitchingNavigation.Infrastructure.Utils;
using System.Diagnostics;
using ViewSwitchingNavigation.Infrastructure.Log;
using Prism.Logging;

namespace DataUsageModule.Services
{
    [Export(typeof(IDataUsageService))]
   public class DataUsageService : IDataUsageService
    {
        long BytesReceivedPrev = 0;
        long BytesSentPrev = 0;
       // Stopwatch sw;

        public DataUsage  GetDataUsageAsync(int seconds)
        {
            return   GetDataUsage(  seconds) ;
        }

        public void  clearDataUsage()
        {
            BytesReceivedPrev = 0;
            BytesSentPrev = 0;
            //if (sw != null) {
            //    sw.Reset();
            //}
        }

        private DataUsage GetDataUsage(int seconds)
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
                   
               CustomLogger.PrintLog("Device Usage: bytesReceived:" + bytesReceived + " bytesSent:"+ bytesSent, Category.Debug, Priority.Low);


                }
            }

            if (BytesReceivedPrev == 0)
            {
                //sw = new Stopwatch();
                //sw.Start();
                BytesReceivedPrev = bytesReceived;
                BytesSentPrev = bytesSent;
                DataUsage.Upload = UtilConvert.SizeToSpeed((bytesSent));
                DataUsage.Download = UtilConvert.SizeToSpeed((bytesReceived));


            }
            else {
                DataUsage.Upload = UtilConvert.SizeToSpeed((long)((bytesSent - BytesSentPrev)/ seconds));
                DataUsage.Download = UtilConvert.SizeToSpeed((long)((bytesReceived - BytesReceivedPrev)/ seconds));
            }

            CustomLogger.PrintLog("Device Usage: Seconds:"+seconds+" bytesReceived:" + UtilConvert.SizeToSpeed((long)((bytesReceived - BytesReceivedPrev) / seconds)) + " bytesSent:" + UtilConvert.SizeToSpeed((long)((bytesSent - BytesSentPrev) / seconds)), Category.Debug, Priority.Low);

            BytesReceivedPrev = bytesReceived;
            BytesSentPrev = bytesSent;
            return DataUsage;
        }
    }
}
