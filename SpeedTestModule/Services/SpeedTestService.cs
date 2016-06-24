using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedTestModule.Model;
using System.ComponentModel.Composition;
using System.Net.NetworkInformation;
using System.Net;
using ViewSwitchingNavigation.Infrastructure.Utils;
using System.IO;
using System.Diagnostics;
using System.Threading;
using ViewSwitchingNavigation.Infrastructure;

namespace SpeedTestModule.Services
{
    [Export(typeof(ISpeedTestService))]
    class SpeedTestService : ISpeedTestService
    {
        UploadSpeedResult uploadResult = null;
        DownloadSpeedResult downloadResult = null;


        List<Uri> UploadFileList = new List<Uri>();
        Dictionary<WebClient, BytesSent> disBytesSent = new Dictionary<WebClient, BytesSent>();

        Dictionary<WebClient, BytesRecieved> disBytesRecieved = new Dictionary<WebClient, BytesRecieved>();
          public long BytesReceivedTotal = 0;
          public long BytesSentTotal = 0;

        public Stopwatch swUpload;
        public Stopwatch swDownload;

        Boolean iscompleted = false;
        Boolean isError = false;

        Boolean isStop = false;

        struct BytesRecieved
        {
            public long BytesReceived;
            public long BytesReceivedPre;
  
        };
        struct BytesSent
        {
            public long TBytesSent;
            public long TBytesSentPre;
 
        };

        public void downloadSpeedFile(DownloadSpeedResult result)
        {
            isStop = false;
            iscompleted = false;
            isError = false;

            downloadResult = result;
            downloadSpeedTest();
        }

        public void GetUploadSpeed(UploadSpeedResult result)
        {
            isStop = false;
            iscompleted = false;
            isError = false;
            uploadResult = result;
            uploadSpeedTest();

        }






        private long CheckBandwidthUsage()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            long bytesReceived = 0;
            foreach (NetworkInterface inf in interfaces)
            {
                if (inf.OperationalStatus == OperationalStatus.Up &&
                    inf.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    inf.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    inf.NetworkInterfaceType != NetworkInterfaceType.Unknown && !inf.IsReceiveOnly)
                {
                    bytesReceived += inf.GetIPv4Statistics().BytesReceived;
                }
            }
            return bytesReceived;
        }

        private long CheckSentBytesUsage()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            long bytesSent = 0;
            foreach (NetworkInterface inf in interfaces)
            {
                if (inf.OperationalStatus == OperationalStatus.Up &&
                    inf.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    inf.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    inf.NetworkInterfaceType != NetworkInterfaceType.Unknown && !inf.IsReceiveOnly)
                {
                    bytesSent += inf.GetIPv4Statistics().BytesSent;
                }
            }
            return bytesSent;
        }




        public bool DeleteFileOnFtpServer(Uri serverUri, string ftpUsername, string ftpPassword)
        {
            try
            {
                // The serverUri parameter should use the ftp:// scheme.
                // It contains the name of the server file that is to be deleted.
                // Example: ftp://contoso.com/someFile.txt.
                // 

                if (serverUri.Scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                //Console.WriteLine("Delete status: {0}", response.StatusDescription);
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void uploadSpeedTest()
        {
            System.Timers.Timer UPTimer = new System.Timers.Timer(1000);
            UPTimer.Elapsed += (s, e) =>
            {

                lock (disBytesSent)
                {

                    SpeedTestUpload Speedresult = new SpeedTestUpload();

                    long BytesSentInSecond = 0;
                    for (int i = 0; i < disBytesSent.Count; i++)
                    {
                        BytesSent byteinfo = disBytesSent.ElementAt(i).Value;
                        if (byteinfo.TBytesSentPre == 0)
                        {
                            byteinfo.TBytesSentPre = byteinfo.TBytesSent;
                            BytesSentInSecond += byteinfo.TBytesSent;
                        }
                        else
                        {

                            BytesSentInSecond += byteinfo.TBytesSent - byteinfo.TBytesSentPre;
                            byteinfo.TBytesSentPre = byteinfo.TBytesSent;
                        }
                        disBytesSent[disBytesSent.ElementAt(i).Key] = byteinfo;

                    }
                    BytesSentTotal += BytesSentInSecond;
                    Speedresult.Upload = UtilConvert.SizeToSpeed(BytesSentInSecond);
                     Speedresult.IsCompleted = false;
                    uploadResult(Speedresult);

                    if (iscompleted)
                    {
                        iscompleted = false;

                        if (UPTimer != null)
                        {
                            UPTimer.Stop();
                            UPTimer.Close();
                            UPTimer.Dispose();
                            UPTimer = null;

                        }
                        this.disBytesSent.Clear();
                        Speedresult.Upload = UtilConvert.SizeToSpeed((long)(BytesSentTotal / swUpload.Elapsed.TotalSeconds));
                        swUpload.Reset();
                        Speedresult.IsCompleted = true;
                        Speedresult.IsError = isError;
                        uploadResult(Speedresult);
                        BytesSentTotal = 0;
                    }

                }
            };



            swUpload = new Stopwatch();
            swUpload.Start();
            for (int i = 0; i < Constants.FTP_ITEREATION; i++)
            {
                Uri URL = new Uri(new Uri(Constants.FTP_URL), +i + "nDoctor" + DateTime.Now.Ticks + "_5mb.zip");
                UploadFileList.Add(URL);
                uploadFTP(URL, Constants.FTP_UPLOAD_FILE, Constants.FTP_USER_NAME, Constants.FTP_PASSWORD);

            }
            UPTimer.AutoReset = true;
            UPTimer.Enabled = true;
            UPTimer.Start();
        }
        public void downloadSpeedTest()
        {
            System.Timers.Timer dTimer = new System.Timers.Timer(1000);
            dTimer.Elapsed += (s, e) =>
           {
               SpeedTestDownload Speedresult = new SpeedTestDownload();

               long BytesReciedInSecond = 0;
               for (int i = 0; i < disBytesRecieved.Count; i++)
               {
                   BytesRecieved byteinfo = disBytesRecieved.ElementAt(i).Value;
                   if (byteinfo.BytesReceivedPre == 0)
                   {
                       byteinfo.BytesReceivedPre = byteinfo.BytesReceived;
                       BytesReciedInSecond += byteinfo.BytesReceived;
                   }
                   else
                   {

                       BytesReciedInSecond += byteinfo.BytesReceived - byteinfo.BytesReceivedPre;
                       byteinfo.BytesReceivedPre = byteinfo.BytesReceived;
                   }
                   disBytesRecieved[disBytesRecieved.ElementAt(i).Key] = byteinfo;

               }


               BytesReceivedTotal += BytesReciedInSecond;
               Speedresult.Download = UtilConvert.SizeToSpeed(BytesReciedInSecond);
                Speedresult.IsCompleted = false;
               downloadResult(Speedresult);

               if (iscompleted)
               {
                   iscompleted = false;

                   if (dTimer != null)
                   {
                       dTimer.Stop();
                       dTimer.Close();
                       dTimer.Dispose();
                       dTimer = null;

                   }


                   Speedresult.Download = UtilConvert.SizeToSpeed((long)(BytesReceivedTotal / swDownload.Elapsed.TotalSeconds));
                   BytesReceivedTotal = 0;
                   swDownload.Reset();
                   Speedresult.IsCompleted = true;
                   Speedresult.IsError = isError;

                   downloadResult(Speedresult);
               }





           };

            Uri URL = new Uri(Constants.FTP_DOWNLOAD_URL);

            String DownloadLocation = UtilFiles.GetTemporaryDirectory();
            swDownload = new Stopwatch();
            swDownload.Start();
            for (int i = 0; i < Constants.FTP_ITEREATION; i++)
            {
                downloadFTP(URL, Constants.FTP_USER_NAME, Constants.FTP_PASSWORD, DownloadLocation + i + "_5MB.zip");

            }

            dTimer.AutoReset = true;
            dTimer.Enabled = true;
            dTimer.Start();
        }

        public void uploadFTP(Uri serverUri, String filename, string username, string password)
        {
            WebClient clientUpload = new WebClient();
            clientUpload.Credentials = new System.Net.NetworkCredential(username, password);
            try
            {
                // client.UploadFileAsync(new Uri(@"ftp://ftpaddress.com" + "/" + new FileInfo(filename).Name), "STOR", filename);
                clientUpload.UploadFileAsync(serverUri, filename);

            }
            catch (System.InvalidCastException)
            {
                // Handled it
            }
            catch (System.ArgumentException)
            {
                // Handled it
            }
            catch (Exception ex)
            {
                // Handled it
            }
            clientUpload.UploadProgressChanged += (s, e) =>
            {

                

                BytesSent byteinfo;

                lock (disBytesSent)
                {
                    if (disBytesSent.TryGetValue((WebClient)s, out byteinfo))
                    {
                        byteinfo.TBytesSent = e.BytesSent;
                        disBytesSent[(WebClient)s] = byteinfo;
                    }
                    else
                    {
                        byteinfo = new BytesSent();
                        byteinfo.TBytesSent = e.BytesSent;
                        byteinfo.TBytesSentPre = 0;
                        disBytesSent.Add((WebClient)s, byteinfo);
                    }
                }

                if (isStop)
                {
                    WebClient webclient = (WebClient)s;
                    webclient.CancelAsync();
                    webclient.Dispose();
                }
            };
            clientUpload.UploadFileCompleted += (s, e) =>
            {
                if (this.disBytesSent.Count == 1)
                {
                    if (!e.Cancelled && e.Error != null)
                    {
                        isError = true;
                    }
                    iscompleted = true;

                    lock (UploadFileList) {
                        for (int i = 0; i < UploadFileList.Count; i++)
                        {
                            bool isDeleted = DeleteFileOnFtpServer(UploadFileList[i], Constants.FTP_USER_NAME, Constants.FTP_PASSWORD);
                            UploadFileList.RemoveAt(i);
                        }
                    }
                        

                }
                 
                    this.disBytesSent.Remove((WebClient)s);

                 



            };
        }
        public void downloadFTP(Uri serverUri, string username, string password, String fileLocation)
        {
            WebClient clientDownload = new WebClient();
            clientDownload.Credentials = new NetworkCredential(username, password);

            try
            {

                clientDownload.DownloadFileAsync(serverUri, fileLocation);

            }
            catch (System.InvalidCastException)
            {

            }
            catch (System.ArgumentException)
            {

            }
            clientDownload.DownloadProgressChanged += (s, e) =>
            {
                
                BytesRecieved byteinfo;
                
                lock (disBytesRecieved)
                {
                    if (disBytesRecieved.TryGetValue((WebClient)s, out byteinfo))
                    {
                        byteinfo.BytesReceived = e.BytesReceived;
                        disBytesRecieved[(WebClient)s] = byteinfo;
                    }
                    else
                    {
                        byteinfo = new BytesRecieved();
                        byteinfo.BytesReceived = e.BytesReceived;
                        byteinfo.BytesReceivedPre = 0;
                        disBytesRecieved.Add((WebClient)s, byteinfo);
                    }
                }

                if (e.TotalBytesToReceive != -1)
                {
                }
                if (isStop)
                {
                    WebClient webclient = (WebClient)s;
                    webclient.CancelAsync();
                    webclient.Dispose();
                }
            };
            clientDownload.DownloadFileCompleted += (s, e) =>
            {
                if (this.disBytesRecieved.Count == 1)
                    {

                        iscompleted = true;
                    if (!e.Cancelled && e.Error != null)
                    {
                        isError = true;
                    }

                }
                    this.disBytesRecieved.Remove((WebClient)s);

            

            };
        }

         


        public void stopSpeedtest()
        {
            isStop = true;
            iscompleted = true;
            for (int i = 0; i < disBytesRecieved.Count(); i++)
                {
                    disBytesRecieved.ElementAt(i).Key.CancelAsync();
                 }

            for (int i = 0; i < disBytesSent.Count(); i++)
            {
                disBytesSent.ElementAt(i).Key.CancelAsync();
            }



            //if (clientDownload != null) {
            //    clientDownload.CancelAsync();
            //    clientDownload = null;
            //    downladResult = null;
            //}
            //if (clientUpload != null)
            //{
            //    clientUpload.CancelAsync();
            //    clientUpload = null;
            //    uploadResult = null;
            //}
            //if(upTimer != null)
            //{
            //    upTimer.Stop();
            //    upTimer.Close();
            //    upTimer.Dispose();

            //}

        }
    }
}
