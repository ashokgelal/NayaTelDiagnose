using Prism.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ViewSwitchingNavigation.Infrastructure.Log;

namespace ViewSwitchingNavigation.Infrastructure.Utils
{
    public class UtilFiles
    {
        static String dataFolder = "NayatelNDoctor";

        public static string GetAppDataDirectory()
        {
            string dataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + dataFolder;
            if(!Directory.Exists(dataDir))
            Directory.CreateDirectory(dataDir);
            return dataDir + Path.DirectorySeparatorChar;
        }

        private static bool DeleteFileOnFtpServer(Uri serverUri, string ftpUsername, string ftpPassword)
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
                CustomLogger.PrintLog("Delete status: " + response.StatusDescription + " File Name:" + serverUri, Category.Debug, Priority.Low);
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                CustomLogger.PrintLog("Delete status: Exception:" + ex.Message + " failed  File Name:" + serverUri, Category.Exception, Priority.Low);

                return false;
            }
        }

        public static void deleteAllFillOnFtp() {
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(Constants.FTP_URL);
                ftpRequest.Credentials = new NetworkCredential(Constants.FTP_USER_NAME, Constants.FTP_PASSWORD);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());

                List<string> directories = new List<string>();

                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                  
                    directories.Add(line);
                    line = streamReader.ReadLine();
                    
                }

                streamReader.Close();

                foreach (var item in directories)
                {
                    if (item == Constants.FTP_DOWNLOAD_FILE_NAME  )
                    {

                    }
                    else
                    {
                        Uri URL = new Uri(new Uri(Constants.FTP_URL), item);
                        DeleteFileOnFtpServer(URL, Constants.FTP_USER_NAME, Constants.FTP_PASSWORD);

                    }
                }

            }
            catch (Exception ex)
            {

                CustomLogger.PrintLog("Delete status: Exception:" + ex.Message  , Category.Exception, Priority.Low);

            }
           
        }

        public static void deleteDW_FTPFileFromAppData()
        {
            String dir = UtilFiles.GetAppDataDirectory();
            System.IO.DirectoryInfo di = new DirectoryInfo(dir);

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.FullName.Contains("100MB.zip"))
                {
                    try
                    {
                        file.Delete();

                    }
                    catch (Exception ex )
                    {
                        CustomLogger.PrintLog("Ftp downloaded file delete:"+ex.Message,Category.Exception,Priority.Low);
                     }

                }
            }

        }
        public static void create5MBFile() {
            String FileName = Constants.FTP_UPLOAD_FILE;
            if (!File.Exists(FileName))
            {
                try { 
               
                        FileStream fs = new FileStream(FileName, FileMode.CreateNew);
                        fs.Seek(5L * 1024 * 1024, SeekOrigin.Begin);
                        fs.WriteByte(0);
                        fs.Close();
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.PrintLog("Dummy File Creation:" + ex.Message, Category.Exception, Priority.High);

                    }



              

            }
        }
    }
}
