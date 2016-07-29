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
using ViewSwitchingNavigation.Infrastructure.Log;
using Prism.Logging;
using System.ComponentModel;

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
        // For cancelling on time out
        List<WebClient> DownloadWebClient = new List<WebClient>();
        List<WebClient> UploadWebClient = new List<WebClient>();
        List<long> DownloadResultPerSecondList = new List<long>();
        List<long> UploadResultPerSecondList = new List<long>();

        static long BytesReceivedTotal = 0;
        public long BytesSentTotal = 0;

        public Stopwatch swUpload;
        public Stopwatch swDownload;

        Boolean iscompleted = false;
        Boolean isError = false;

        Boolean isStop = false;
        Boolean IsdownloadStart = false;
        Boolean IsUploadStart = false;
        Boolean isInTimer = false;

        
        System.Timers.Timer dTimer;
        System.Timers.Timer UPTimer;
        int fileDownloadCompleted = 0;
        int fileUploadCompleted = 0;

        // for system bandwith
          long BytesReceivedPre;
          long BytesSentPre;


        struct BytesRecieved
        {
            public long BytesReceived;
            public long BytesReceivedPre;
            public long TotalBytesToReceive;

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
            BytesReceivedPre = 0;
            downloadResult = result;
           // downloadSpeedTest();
            downloadSpeedTestBySystem();
        }

        public void GetUploadSpeed(UploadSpeedResult result)
        {
            BytesSentPre = 0;

            isStop = false;
            iscompleted = false;
            isError = false;
            uploadResult = result;
            //uploadSpeedTest();
            uploadSpeedTestBySystem();
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
                CustomLogger.PrintLog("Delete status: "+ response.StatusDescription +" File Name:"+ serverUri, Category.Debug, Priority.Low);
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                CustomLogger.PrintLog("Delete status: Exception:"+ex.Message+" failed  File Name:" + serverUri, Category.Exception, Priority.Low);

                return false;
            }
        }
        /// <summary>
        /// test speed by internal file downloading and uploading system
        /// </summary>
        public void uploadSpeedTest()
        {
            IsUploadStart = false;
            UPTimer = new System.Timers.Timer(1000);
            UPTimer.Elapsed += (s, e) =>
            {

                lock (disBytesSent)
                {

                    SpeedTestUpload Speedresult = new SpeedTestUpload();
                    Dictionary<WebClient, BytesSent> Dic = new Dictionary<WebClient, BytesSent>(disBytesSent);

                    long BytesSentInSecond = 0;
                    for (int i = 0; i < Dic.Count; i++)
                    {
                        BytesSent byteinfo = Dic.ElementAt(i).Value;
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
                    UploadResultPerSecondList.Add(BytesSentInSecond);
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

                        long speed = 0;
                        foreach (var item in UploadResultPerSecondList)
                        {
                            speed += item;
                        }

                        CustomLogger.PrintLog("Upload Test Completed: BytesSentTotal:" + BytesSentTotal + " TotalSeconds:" + swUpload.Elapsed.TotalSeconds + "  In Mbs:  " + UtilConvert.SizeToSpeed((long)(BytesSentTotal / swUpload.Elapsed.TotalSeconds)), Category.Debug, Priority.Low);
                        CustomLogger.PrintLog("Upload Test Completed: BytesReceivedTotal:" + speed + " TotalSeconds:" + UploadResultPerSecondList.Count() + "  In Mbs:  " + UtilConvert.SizeToSpeed((long)(speed / UploadResultPerSecondList.Count)), Category.Debug, Priority.Low);

                        //Speedresult.Upload = UtilConvert.SizeToSpeed((long)(BytesSentTotal / swUpload.Elapsed.TotalSeconds));
                        Speedresult.Upload = UtilConvert.SizeToSpeed((long)(speed / UploadResultPerSecondList.Count));

                        swUpload.Reset();
                        UploadResultPerSecondList.Clear();
                        Speedresult.IsCompleted = true;
                        Speedresult.IsError = isError;
                        uploadResult(Speedresult);
                        BytesSentTotal = 0;
                        removeUploadFtpFiles();
                    }

                }
            };

            disBytesSent = new Dictionary<WebClient, BytesSent>();
            UploadWebClient = new List<WebClient>();
            UploadResultPerSecondList = new List<long>();
            UploadFileList = new List<Uri>();
            fileUploadCompleted = 0;
            BytesSentTotal = 0;
            swUpload = new Stopwatch();
            swUpload.Start();
            for (int i = 0; i < Constants.FTP_ITEREATION_UPLOAD; i++)
            {
                Thread.Sleep(50);
                Uri URL = new Uri(new Uri(Constants.FTP_URL), +i + "nDoctor" + DateTime.Now.Ticks + "_5mb.zip");
                UploadFileList.Add(URL);
                uploadFTP(URL, Constants.FTP_UPLOAD_FILE, Constants.FTP_USER_NAME, Constants.FTP_PASSWORD,false);

            }
           
        }
        public void downloadSpeedTest()
        {
            IsdownloadStart = false;
            dTimer = new System.Timers.Timer(1000);
            dTimer.Elapsed += (s, e) =>
           {
               isInTimer = true;
               lock (disBytesRecieved)
               {

               

               SpeedTestDownload Speedresult = new SpeedTestDownload();

               long BytesReciedInSecond = 0;
                   Dictionary<WebClient, BytesRecieved> Dic = new Dictionary<WebClient, BytesRecieved>(disBytesRecieved);
 
                   for (int i = 0; i < Dic.Count; i++)
               {
                   BytesRecieved byteinfo = Dic.ElementAt(i).Value;
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
                   DownloadResultPerSecondList.Add(BytesReciedInSecond);
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

                       CustomLogger.PrintLog("Download Test Completed: BytesReceivedTotal:" + BytesReceivedTotal + " TotalSeconds:" + swDownload.Elapsed.TotalSeconds+ "  In Mbs:  " +UtilConvert.SizeToSpeed((long)(BytesReceivedTotal / swDownload.Elapsed.TotalSeconds)), Category.Debug, Priority.Low);
                       double seconds = swDownload.Elapsed.TotalSeconds;

                    DownloadResultPerSecondList.Sort();


                       //for (int i = 0; i < 3; i++)
                       //{
                       //    DownloadResultPerSecondList.RemoveAt(i);
                       //}
                       //int count = DownloadResultPerSecondList.Count();
                       //for (int i = count - 1; i > count-4; i--)
                       //{
                       //    DownloadResultPerSecondList.RemoveAt(i);
                       //}

                       // Speedresult.Download = UtilConvert.SizeToSpeed((long)(BytesReceivedTotal / swDownload.Elapsed.TotalSeconds));
                       long speed = 0;
                       foreach (var item in DownloadResultPerSecondList)
                       {
                           speed += item;
                       }

                       Speedresult.Download = UtilConvert.SizeToSpeed((long)(speed / DownloadResultPerSecondList.Count));
                       CustomLogger.PrintLog("Download Test Completed: BytesReceivedTotal:" + speed + " TotalSeconds:" + DownloadResultPerSecondList.Count + "  In Mbs:  " + UtilConvert.SizeToSpeed((long)(speed / DownloadResultPerSecondList.Count)), Category.Debug, Priority.Low);

                       BytesReceivedTotal = 0;
                   swDownload.Reset();
                   Speedresult.IsCompleted = true;
                   Speedresult.IsError = isError;
                   DownloadResultPerSecondList.Clear();
                   downloadResult(Speedresult);
               }

               }// lock end

               isInTimer = false;


           };

            Uri URL = new Uri(Constants.FTP_DOWNLOAD_URL);
            String DownloadLocation = UtilFiles.GetAppDataDirectory();
           disBytesRecieved = new Dictionary<WebClient, BytesRecieved>();
            DownloadWebClient = new List<WebClient>();
            DownloadResultPerSecondList = new List<long>();
            fileDownloadCompleted = 0;
            swDownload = new Stopwatch();
            swDownload.Start();
            for (int i = 0; i < Constants.FTP_ITEREATION_DOWNLOAD; i++)
            {
                Thread.Sleep(50);
                downloadFTP(URL, Constants.FTP_USER_NAME, Constants.FTP_PASSWORD, DownloadLocation + i + "_100MB.zip",false);

            }

            
        }


        /// <summary>
        /// test speed by System bandwith sent and recieve
        /// </summary>
         
        public void downloadSpeedTestBySystem()
        {
            
             IsdownloadStart = false;
            dTimer = new System.Timers.Timer(1000);
            dTimer.Elapsed += (s, e) =>
            {
                  
                SpeedTestDownload Speedresult = new SpeedTestDownload();

                long BytesReciedInSecond = 0;
                long BytesReceived = CheckBandwidthUsage(); 
                 BytesReciedInSecond =  BytesReceived -  BytesReceivedPre;
                 BytesReceivedPre = BytesReceived;
                  
                    BytesReceivedTotal += BytesReciedInSecond;
                    Speedresult.Download = UtilConvert.SizeToSpeed(BytesReciedInSecond);
                    Speedresult.IsCompleted = false;
                    downloadResult(Speedresult);
                    DownloadResultPerSecondList.Add(BytesReciedInSecond);
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

                        CustomLogger.PrintLog("Download Test Completed: BytesReceivedTotal:" + BytesReceivedTotal + " TotalSeconds:" + swDownload.Elapsed.TotalSeconds + "  In Mbs:  " + UtilConvert.SizeToSpeed((long)(BytesReceivedTotal / swDownload.Elapsed.TotalSeconds)), Category.Debug, Priority.Low);
                        double seconds = swDownload.Elapsed.TotalSeconds;

                        DownloadResultPerSecondList.Sort();


                        long speed = 0;
                        foreach (var item in DownloadResultPerSecondList)
                        {
                            speed += item;
                        }

                        Speedresult.Download = UtilConvert.SizeToSpeed((long)(speed / DownloadResultPerSecondList.Count));
                        CustomLogger.PrintLog("Download Test Completed: BytesReceivedTotal:" + speed + " TotalSeconds:" + DownloadResultPerSecondList.Count + "  In Mbs:  " + UtilConvert.SizeToSpeed((long)(speed / DownloadResultPerSecondList.Count)), Category.Debug, Priority.Low);

                        BytesReceivedTotal = 0;
                        swDownload.Reset();
                        Speedresult.IsCompleted = true;
                        Speedresult.IsError = isError;
                        DownloadResultPerSecondList.Clear();
                        downloadResult(Speedresult);
                    } 
            };
            Uri URL = new Uri(Constants.FTP_DOWNLOAD_URL);
            String DownloadLocation = UtilFiles.GetAppDataDirectory();
            disBytesRecieved = new Dictionary<WebClient, BytesRecieved>();
            DownloadWebClient = new List<WebClient>();
            DownloadResultPerSecondList = new List<long>();
            fileDownloadCompleted = 0;
            swDownload = new Stopwatch();
            swDownload.Start();
            for (int i = 0; i < Constants.FTP_ITEREATION_DOWNLOAD; i++)
            {
                Thread.Sleep(50);
                downloadFTP(URL, Constants.FTP_USER_NAME, Constants.FTP_PASSWORD, DownloadLocation + i + "_100MB.zip",true);

            }


        }

        public void uploadSpeedTestBySystem()
        {  
            IsUploadStart = false;
            UPTimer = new System.Timers.Timer(1000);
            UPTimer.Elapsed += (s, e) =>
            {      
            SpeedTestUpload Speedresult = new SpeedTestUpload();
            long BytesSent = CheckSentBytesUsage();
            long BytesSentInSecond  =  BytesSent -  BytesSentPre;
                BytesSentPre = BytesSent;
                BytesSentTotal += BytesSentInSecond;

                    UploadResultPerSecondList.Add(BytesSentInSecond);
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

                        long speed = 0;
                        foreach (var item in UploadResultPerSecondList)
                        {
                            speed += item;
                        }

                        CustomLogger.PrintLog("Upload Test Completed: BytesSentTotal:" + BytesSentTotal + " TotalSeconds:" + swUpload.Elapsed.TotalSeconds + "  In Mbs:  " + UtilConvert.SizeToSpeed((long)(BytesSentTotal / swUpload.Elapsed.TotalSeconds)), Category.Debug, Priority.Low);
                        CustomLogger.PrintLog("Upload Test Completed: BytesReceivedTotal:" + speed + " TotalSeconds:" + UploadResultPerSecondList.Count() + "  In Mbs:  " + UtilConvert.SizeToSpeed((long)(speed / UploadResultPerSecondList.Count)), Category.Debug, Priority.Low);

                        //Speedresult.Upload = UtilConvert.SizeToSpeed((long)(BytesSentTotal / swUpload.Elapsed.TotalSeconds));
                        Speedresult.Upload = UtilConvert.SizeToSpeed((long)(speed / UploadResultPerSecondList.Count));

                        swUpload.Reset();
                        UploadResultPerSecondList.Clear();
                        Speedresult.IsCompleted = true;
                        Speedresult.IsError = isError;
                        uploadResult(Speedresult);
                        BytesSentTotal = 0;
                        removeUploadFtpFiles();
                    }

                
            };

            disBytesSent = new Dictionary<WebClient, BytesSent>();
            UploadWebClient = new List<WebClient>();
            UploadResultPerSecondList = new List<long>();
            fileUploadCompleted = 0;
            BytesSentTotal = 0;
            swUpload = new Stopwatch();
            swUpload.Start();
            for (int i = 0; i < Constants.FTP_ITEREATION_UPLOAD; i++)
            {
                Thread.Sleep(50);
                Uri URL = new Uri(new Uri(Constants.FTP_URL), +i + "nDoctor" + DateTime.Now.Ticks + "_5mb.zip");
                UploadFileList.Add(URL);
                uploadFTP(URL, Constants.FTP_UPLOAD_FILE, Constants.FTP_USER_NAME, Constants.FTP_PASSWORD,true);

            }

        }


        public void uploadFTP(Uri serverUri, String filename, string username, string password,Boolean isBySystem)
        {
            WebClient clientUpload = null;
            try
            {
                clientUpload = new WebClient();
                clientUpload.Credentials = new System.Net.NetworkCredential(username, password);
                clientUpload.UploadFileAsync(serverUri, null, filename, fileUploadCompleted.ToString());  
                

            }
            catch (WebException ex)
            {
                CustomLogger.PrintLog(" : FileId:" + fileUploadCompleted.ToString() + "Exception:" + ex.Message, Category.Exception, Priority.High);

            }
            catch (System.InvalidCastException ex)
            {
                // Handled it
                CustomLogger.PrintLog(" : FileId:" + fileUploadCompleted.ToString() + "Exception:" + ex.Message, Category.Exception, Priority.High);

            }
            catch (System.ArgumentException ex)
            {
                // Handled it
                CustomLogger.PrintLog(" : FileId:" + fileUploadCompleted.ToString() + "Exception:" + ex.Message, Category.Exception, Priority.High);

            }
             
            
            CustomLogger.PrintLog("uploadFileStarting: FileId:" + fileUploadCompleted.ToString() + " filename:" + filename, Category.Debug, Priority.Low);
            fileUploadCompleted++;
            UploadWebClient.Add(clientUpload);

            clientUpload.UploadProgressChanged += (s, e) =>
            {

                if (UPTimer != null && !IsUploadStart)
                {
                    UPTimer.AutoReset = true;
                    UPTimer.Enabled = true;
                    UPTimer.Start();
                    IsUploadStart = true;
                    if (isBySystem)
                        BytesSentPre = CheckSentBytesUsage();
                }
                if (isStop)
                {
                    WebClient webclient = (WebClient)s;
                    webclient.CancelAsync();
                    webclient.Dispose();
                }
                if (isBySystem)
                    return;

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

                
            };
            clientUpload.UploadFileCompleted += (s, e) =>
            {
                String FileId = e.UserState!=null? e.UserState.ToString():"Not Found";

                if (e.Error != null)
                    CustomLogger.PrintLog("UploadFileCompleted:  FileId: " + FileId+"  With Error:" + e.Error  , Category.Debug, Priority.Low);
                else
                    CustomLogger.PrintLog("UploadFileCompleted: FileId:" + FileId, Category.Debug, Priority.Low);

                fileUploadCompleted--;
                if (fileUploadCompleted == 0)
                {
                    if (!e.Cancelled && e.Error != null)
                    {
                        isError = true;
                    }
                    iscompleted = true;

                    removeUploadFtpFiles();



                }
                 
 
                 



            };
        }

        void removeLocalFile() {

            BackgroundWorker backgroundWorker = new BackgroundWorker();


            backgroundWorker.DoWork += delegate
            {

                UtilFiles.deleteDW_FTPFileFromAppData();
            };
            backgroundWorker.RunWorkerAsync();
        }


       void removeUploadFtpFiles()
        {
            List<Uri> UploadFileList1 = new List<Uri>(UploadFileList);

            foreach (var item in UploadFileList1)
            {
              DeleteFileOnFtpServer(item,Constants.FTP_USER_NAME,Constants.FTP_PASSWORD);
            }
            UploadFileList.Clear();
        }

        public void downloadFTP(Uri serverUri, string username, string password, String fileLocation, Boolean isBySystem)
        {
            WebClient clientDownload = new WebClient();
            clientDownload.Credentials = new NetworkCredential(username, password);

            try
            {
                CustomLogger.PrintLog("DownloadFileStarting: FileId:" + fileDownloadCompleted.ToString(), Category.Debug, Priority.Low);
                clientDownload.DownloadFileAsync(serverUri, fileLocation, fileDownloadCompleted.ToString());
                fileDownloadCompleted++;
                DownloadWebClient.Add(clientDownload);
            }
            catch (System.InvalidCastException)
            {

            }
            catch (System.ArgumentException)
            {

            }
            clientDownload.DownloadProgressChanged += (s, e) =>
            {

                if (dTimer != null && !IsdownloadStart) {
                    dTimer.AutoReset = true;
                    dTimer.Enabled = true;
                    dTimer.Start();
                    IsdownloadStart = true;
                    if(isBySystem)
                        BytesReceivedPre = CheckBandwidthUsage();
                }
                if (isStop)
                {
                    WebClient webclient = (WebClient)s;
                    webclient.CancelAsync();
                    webclient.Dispose();
                }
                if (isBySystem)
                    return;
                BytesRecieved byteinfo;
                 lock (disBytesRecieved)
                {
                    if (disBytesRecieved.TryGetValue((WebClient)s, out byteinfo))
                    {
                        byteinfo.BytesReceived = e.BytesReceived;
                        byteinfo.TotalBytesToReceive = e.BytesReceived;
                        disBytesRecieved[(WebClient)s] = byteinfo;
                    }
                    else
                    {
                        
                        byteinfo = new BytesRecieved();
                        byteinfo.BytesReceived = e.BytesReceived;
                        byteinfo.BytesReceivedPre = 0;
                        byteinfo.TotalBytesToReceive = e.BytesReceived;
                        disBytesRecieved.Add((WebClient)s, byteinfo);
                    }
                }

                 
               
            };
            clientDownload.DownloadFileCompleted += (s, e) =>
            {
                String FileId = e.UserState != null ? e.UserState.ToString() : "Not Found";
                WebClient clientFTP = (WebClient)s;
                 
                if (e.Error != null)
                    CustomLogger.PrintLog("DownloadFileCompleted: With Error:" + e.Error + " FileId:" + FileId, Category.Debug, Priority.Low);
                else
                    CustomLogger.PrintLog("DownloadFileCompleted: FileId:" + FileId, Category.Debug, Priority.Low);


                fileDownloadCompleted--;
                lock (disBytesRecieved)
                {
                    if (fileDownloadCompleted == 0)
                    {

                        iscompleted = true;
                        if (!e.Cancelled && e.Error != null)
                        {
                            isError = true;
                        }

                    }
                  
                   // 

                }

            };
        }


        public void StopThreadsList() {

            for (int i = 0; i < disBytesRecieved.Count(); i++)
            {
                disBytesRecieved.ElementAt(i).Key.CancelAsync();
            }

            for (int i = 0; i < disBytesSent.Count(); i++)
            {
                disBytesSent.ElementAt(i).Key.CancelAsync();
            }


            for (int i = 0; i < DownloadWebClient.Count(); i++)
            {
                DownloadWebClient.ElementAt(i).CancelAsync();
            }

            for (int i = 0; i < UploadWebClient.Count(); i++)
            {
                UploadWebClient.ElementAt(i).CancelAsync();
            }
            disBytesRecieved.Clear();
            disBytesSent.Clear();
            DownloadWebClient.Clear();
            UploadWebClient.Clear();
        }

        public void stopSpeedtest()
        {
            isStop = true;
            iscompleted = true;
            StopThreadsList();
            removeLocalFile();
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
