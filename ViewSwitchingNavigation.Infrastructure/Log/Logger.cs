using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
namespace ViewSwitchingNavigation.Infrastructure.Log
{
    class Logger
    {

        //internal static string logFileName = "";

        //private static FileStream logFileStream;
        //private static StreamWriter logWriter;

        //internal static void CreateLogFile()
        //{
        //    string dir = Path.Combine(Directory.GetCurrentDirectory(), "logs");

        //    if (!Directory.Exists(dir))
        //        Directory.CreateDirectory(dir);

        //    DateTime now = DateTime.Now;
        //    string filename = string.Format("logger-{0:dd\\-MM\\-yyyy\\-HH\\-mm\\-ss}", now);

        //    if (File.Exists(Path.Combine(dir, filename + ".log")))
        //    {
        //        for (int i = 1; ; i++)
        //        {
        //            string tmpPath = Path.Combine(dir, filename + "-" + i + ".log");
        //            if (!File.Exists(tmpPath))
        //            {
        //                logFileName = tmpPath;
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        logFileName = Path.Combine(dir, filename + ".log");
        //    }
        //}

        //internal static void OpenFile()
        //{
        //    logFileStream = new FileStream(logFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        //    logWriter = new StreamWriter(logFileStream);

        //    logWriter.AutoFlush = true;
        //}

        //internal static void CloseFile()
        //{
        //    logWriter.Flush();
        //    logWriter.Close();
        //    logWriter.Dispose();
        //    logWriter = null;

        //    logFileStream.Dispose();
        //    logFileStream = null;
        //}

        //#region Log

        ///// <summary>
        ///// Create a new log entry
        ///// </summary>
        ///// <param name="priority">The priority of the log</param>
        ///// <param name="message">The message</param>
        //public static void Log(LogPriority priority, string message)
        //{
        //    Log(priority, message, "nettools", LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry
        ///// </summary>
        ///// <param name="priority">The priority of the log</param>
        ///// <param name="message">The message</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Log(LogPriority priority, string message, LogMethod method)
        //{
        //    Log(priority, message, "nettools", method);
        //}

        ///// <summary>
        ///// Create a new log entry
        ///// </summary>
        ///// <param name="priority">The priority of the log</param>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        //public static void Log(LogPriority priority, string message, string prefix)
        //{
        //    Log(priority, message, prefix, LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry
        ///// </summary>
        ///// <param name="priority">The priority of the log</param>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Log(LogPriority priority, string message, string prefix, LogMethod method)
        //{
        //    string dateString = "[" + DateTime.Now.ToLongTimeString() + "]";
        //    string priorityString = "[" + priority.ToString().ToUpper() + "]";
        //    string prefixString = "[" + prefix.Trim(new char[] { '[', ']', '(', ')', '{', '}' }).ToUpper() + "]";

        //    string log = dateString + priorityString + prefixString + " " + message.Trim(new char[] { '\n', '\r', '\0' });

        //    if (method == LogMethod.Full)
        //    {
        //        logWriter.WriteLine(log);
        //        Console.WriteLine(log);
        //    }
        //    else if (method == LogMethod.ConsoleOnly)
        //    {
        //        Console.WriteLine(log);
        //    }
        //    else if (method == LogMethod.FileOnly)
        //    {
        //        logWriter.WriteLine(log);
        //    }
        //}

        //#endregion

        //#region Info

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Info"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        //public static void Info(string message)
        //{
        //    Log(LogPriority.Info, message, "nettools", LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Info"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Info(string message, LogMethod method)
        //{
        //    Log(LogPriority.Info, message, "nettools", method);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Info"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        //public static void Info(string message, string prefix)
        //{
        //    Log(LogPriority.Info, message, prefix, LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Info"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Info(string message, string prefix, LogMethod method)
        //{
        //    Log(LogPriority.Info, message, prefix, method);
        //}

        //#endregion

        //#region Debug

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Info"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        //public static void Debug(string message)
        //{
        //    if (Program.__debug)
        //        Log(LogPriority.Debug, message, "nettools", LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Info"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Debug(string message, LogMethod method)
        //{
        //    if (Program.__debug)
        //        Log(LogPriority.Debug, message, "nettools", method);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Info"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        //public static void Debug(string message, string prefix)
        //{
        //    if (Program.__debug)
        //        Log(LogPriority.Debug, message, prefix, LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Info"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Debug(string message, string prefix, LogMethod method)
        //{
        //    if (Program.__debug)
        //        Log(LogPriority.Debug, message, prefix, method);
        //}

        //#endregion

        //#region Warning

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Warning"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        //public static void Warning(string message)
        //{
        //    Log(LogPriority.Warning, message, "nettools", LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Warning"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Warning(string message, LogMethod method)
        //{
        //    Log(LogPriority.Warning, message, "nettools", method);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Warning"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        //public static void Warning(string message, string prefix)
        //{
        //    Log(LogPriority.Warning, message, prefix, LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Warning"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Warning(string message, string prefix, LogMethod method)
        //{
        //    Log(LogPriority.Warning, message, prefix, method);
        //}

        //#endregion

        //#region Error

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Error"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        //public static void Error(string message)
        //{
        //    Log(LogPriority.Error, message, "nettools", LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Error"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Error(string message, LogMethod method)
        //{
        //    Log(LogPriority.Error, message, "nettools", method);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Error"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        //public static void Error(string message, string prefix)
        //{
        //    Log(LogPriority.Error, message, prefix, LogMethod.Full);
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Error"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Error(string message, string prefix, LogMethod method)
        //{
        //    Log(LogPriority.Error, message, prefix, method);
        //}

        //#endregion

        //#region Exception

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Exception"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        //public static void Exception(Exception ex)
        //{
        //    StackFrame[] frames = new StackTrace(ex, true).GetFrames();
        //    StackFrame file = frames.First(t => t.GetFileName() != null);

        //    if (file.GetFileName().EndsWith("Command.cs"))
        //    {
        //        Log(LogPriority.Exception, ex.Message + " In file " + Path.GetFileName(file.GetFileName()) + " on line " + file.GetFileLineNumber(),
        //            Path.GetFileName(file.GetFileName()).Replace("Command.cs", ""), LogMethod.Full);
        //        logWriter.WriteLine(ex.StackTrace);
        //    }
        //    else
        //    {
        //        Log(LogPriority.Exception, ex.Message + " In file " + Path.GetFileName(file.GetFileName()) + " on line " + file.GetFileLineNumber(), "nettools", LogMethod.Full);
        //        logWriter.WriteLine(ex.StackTrace);
        //    }
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Exception"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Exception(Exception ex, LogMethod method)
        //{
        //    StackFrame[] frames = new StackTrace(ex, true).GetFrames();
        //    StackFrame file = frames.First(t => t.GetFileName() != null);

        //    if (file.GetFileName().EndsWith("Command.cs"))
        //    {
        //        Log(LogPriority.Exception, ex.Message + " In file " + Path.GetFileName(file.GetFileName()) + " on line " + file.GetFileLineNumber(),
        //            Path.GetFileName(file.GetFileName()).Replace("Command.cs", ""), LogMethod.Full);
        //        logWriter.WriteLine(ex.StackTrace);
        //    }
        //    else
        //    {
        //        Log(LogPriority.Exception, ex.Message + " In file " + file.GetFileName() + " on line " + file.GetFileLineNumber(), "nettools", LogMethod.Full);
        //        logWriter.WriteLine(ex.StackTrace);
        //    }
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Exception"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        //public static void Exception(Exception ex, string prefix)
        //{
        //    StackFrame[] frames = new StackTrace(ex, true).GetFrames();
        //    StackFrame file = frames.First(t => t.GetFileName() != null);

        //    if (file.GetFileName().EndsWith("Command.cs"))
        //    {
        //        Log(LogPriority.Exception, ex.Message + " In file " + Path.GetFileName(file.GetFileName()) + " on line " + file.GetFileLineNumber(),
        //            Path.GetFileName(file.GetFileName()).Replace("Command.cs", ""), LogMethod.Full);
        //        logWriter.WriteLine(ex.StackTrace);
        //    }
        //    else
        //    {
        //        Log(LogPriority.Exception, ex.Message + " In file " + file.GetFileName() + " on line " + file.GetFileLineNumber(), "nettools", LogMethod.Full);
        //        logWriter.WriteLine(ex.StackTrace);
        //    }
        //}

        ///// <summary>
        ///// Create a new log entry using <seealso cref="LogPriority.Exception"/> as priority
        ///// </summary>
        ///// <param name="message">The message</param>
        ///// <param name="prefix">The prefix</param>
        ///// <param name="method">The method how it should be logged</param>
        //public static void Exception(Exception ex, string prefix, LogMethod method)
        //{
        //    StackFrame[] frames = new StackTrace(ex, true).GetFrames();
        //    StackFrame file = frames.First(t => t.GetFileName() != null);

        //    if (file.GetFileName().EndsWith("Command.cs"))
        //    {
        //        Log(LogPriority.Exception, ex.Message + " In file " + Path.GetFileName(file.GetFileName()) + " on line " + file.GetFileLineNumber(),
        //            Path.GetFileName(file.GetFileName()).Replace("Command.cs", ""), LogMethod.Full);
        //        logWriter.WriteLine(ex.StackTrace);
        //    }
        //    else
        //    {
        //        Log(LogPriority.Exception, ex.Message + " In file " + file.GetFileName() + " on line " + file.GetFileLineNumber(), "nettools", LogMethod.Full);
        //        logWriter.WriteLine(ex.StackTrace);
        //    }
        //}

        //#endregion

    }

    public enum LogMethod
    {

        /// <summary>
        /// Print the log to the console and put it into the logfile
        /// </summary>
        Full = ConsoleOnly | FileOnly,

        /// <summary>
        /// Print the log to the console only
        /// </summary>
        ConsoleOnly = 1,

        /// <summary>
        /// Put the log into the logfile only
        /// </summary>
        FileOnly = 2
    }

    public enum LogPriority
    {

        /// <summary>
        /// The log if only a info
        /// </summary>
        Info = 0,

        /// <summary>
        /// The log is a warning (eg. Wrong configurations)
        /// </summary>
        Warning = 1,

        /// <summary>
        /// The log is an error (eg. Missing referenced assembly)
        /// </summary>
        Error = 2,

        /// <summary>
        /// The log is an exception (Stack-Trace will be saved to the log-file only)
        /// </summary>
        Exception = 3,

        /// <summary>
        /// The log is an debug-message
        /// </summary>
        Debug = 4
    }

}
