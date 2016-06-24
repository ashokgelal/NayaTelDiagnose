using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace nettools.Utils
{

    internal static class DownloadUtils
    {

        public static void DownloadFile(Uri url, int timeout = 10000, string method = "GET", string user = null, string pass = "")
        {
            string dir = Directory.GetCurrentDirectory();
            string fileName = Path.GetFileName(url.AbsolutePath);

            FileInfo file = new FileInfo(Path.Combine(dir, fileName));

            WebRequest request = WebRequest.Create(url);
            request.Timeout = timeout;
            request.Method = method;

            if (user != null)
                request.Credentials = new NetworkCredential(user, pass);
            else
                request.UseDefaultCredentials = true;

            WebResponse response = request.GetResponse();

            Stream networkStream = response.GetResponseStream();
            FileStream localStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);

            long current = 0;
            long total = response.ContentLength;

            byte[] buffer = new byte[1024];

            Stopwatch watch = new Stopwatch();
            long tmpCurrent = 0;

            double speed = 0.0;
            double time = 0.0;

            do
            {
                int read = networkStream.Read(buffer, 0, buffer.Length);
                localStream.Write(buffer, 0, read);

                current += read;
                tmpCurrent += read;

                if (watch.Elapsed.Seconds >= 1)
                {
                    speed = (double)(tmpCurrent / (watch.ElapsedMilliseconds));
                    time = Math.Round(((total - current) / 1024) / speed, 0);

                    watch.Reset();
                    tmpCurrent = 0;

                    ConsoleProgressBar.DrawProgressBar(current, total, 100, speed, time);

                    watch.Start();
                }
            }
            while (current < total);
        }

    }

}