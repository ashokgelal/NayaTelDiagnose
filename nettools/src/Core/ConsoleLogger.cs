using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace nettools.Core
{

    public class ConsoleLogger
    {

        internal static string logFileName = "";

        private static FileStream logFileStream;
        private static StreamWriter logWriter;

        private static DoubleWriter outWriter;

        internal static void CreateLogFile()
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "logs");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            DateTime now = DateTime.Now;
            string filename = string.Format("console-{0:dd\\-MM\\-yyyy\\-HH\\-mm\\-ss}", now);

            if (File.Exists(Path.Combine(dir, filename + ".log")))
            {
                for (int i = 1; ; i++)
                {
                    string tmpPath = Path.Combine(dir, filename + "-" + i + ".log");
                    if (!File.Exists(tmpPath))
                    {
                        logFileName = tmpPath;
                        break;
                    }
                }
            }
            else
            {
                logFileName = Path.Combine(dir, filename + ".log");
            }
        }

        internal static void OpenFile()
        {
            logFileStream = new FileStream(logFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            logWriter = new StreamWriter(logFileStream);

            TextWriter oldOut = Console.Out;
            TextReader oldIn = Console.In;

            try
            {
                outWriter = new DoubleWriter(logWriter, oldOut);
            }
            catch (Exception)
            {
                return;
            }

            Console.SetOut(outWriter);

            logWriter.AutoFlush = true;
        }

        internal static void CloseFile()
        {
            logWriter.Flush();
            logWriter.Close();
            logWriter.Dispose();
            logWriter = null;

            logFileStream.Dispose();
            logFileStream = null;

            if (outWriter != null)
            {
                outWriter.Dispose();
                outWriter = null;
            }
        }

        private class DoubleWriter : TextWriter
        {

            TextWriter one;
            TextWriter two;

            public DoubleWriter(TextWriter one, TextWriter two)
            {
                this.one = one;
                this.two = two;
            }

            public override Encoding Encoding
            {
                get { return one.Encoding; }
            }

            public override void Flush()
            {
                one.Flush();
                two.Flush();
            }

            public override void Write(char value)
            {
                one.Write(value);
                two.Write(value);
            }

        }
        
    }
    
}
