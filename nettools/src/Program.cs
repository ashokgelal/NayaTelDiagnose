using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;

using nettools.Configuration;
using nettools.Core;
using nettools.Plugins;
using nettools.Cryptography.Hashing;

using OpenSSL.Core;
using OpenSSL.Crypto;
using OpenSSL.X509;

namespace nettools
{

    public class Program
	{

        internal static bool __debug = false;

        internal static Thread __workingThread = null;
        internal static ICommand __workingCommand = null;
        private static ManualResetEvent waiter = new ManualResetEvent(false);

        internal static string __currentDirectory = Directory.GetCurrentDirectory();
        internal static readonly string __applictionDirectory = AppDomain.CurrentDomain.BaseDirectory;

        internal static IWebProxy __proxy = null;
        internal static string __userCertName = "";
        internal static X509Certificate __userCert = null;

        internal static Dictionary<Type, IPlugin> __pluginsTypes = new Dictionary<Type, IPlugin>();
        internal static Dictionary<IPlugin, IConfiguration> __pluginsConfiguration = new Dictionary<IPlugin, IConfiguration>();

        public static IWebProxy Proxy
        {
            get
            {
                return __proxy;
            }
        }

        public static bool IsDebugMode
        {
            get
            {
                return __debug;
            }
        }
        
        public static Thread WorkingThread
        {
            get
            {
                return __workingThread;
            }
        }
        
        public static string CurrentDirectory
        {
            get
            {
                return __currentDirectory;
            }
        }
        
        public static string ApplicationDirectory
        {
            get
            {
                return __applictionDirectory;
            }
        }

        private static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Clear();

            PrintCopyright(true);

            Console.Write(new string('=', 50));
            Console.Write("\n=" + new string(' ', 48) + "=\n");
            Console.Write("=" + new string(' ', 14) + "Welcome to nettools " + new string(' ', 14) + "=");
            Console.Write("\n=" + new string(' ', 48) + "=\n");
            Console.Write(new string('=', 50));

            bool finished = false;

            new Thread(() =>
            {
                Logger.CreateLogFile();
                ConsoleLogger.CreateLogFile();

                Logger.OpenFile();
                ConsoleLogger.OpenFile();

                if (!Directory.Exists(__applictionDirectory + "\\plugins"))
                    Directory.CreateDirectory(__applictionDirectory + "\\plugins");

                if (!Directory.Exists(__applictionDirectory + "\\plugins\\compiled"))
                    Directory.CreateDirectory(__applictionDirectory + "\\plugins\\compiled");

                if (!Directory.Exists(__applictionDirectory + "\\plugins\\libraries"))
                    Directory.CreateDirectory(__applictionDirectory + "\\plugins\\libraries");

                if (!Directory.Exists(__applictionDirectory + "\\plugins\\config"))
                    Directory.CreateDirectory(__applictionDirectory + "\\plugins\\config");

                if (!Directory.Exists(__applictionDirectory + "\\data"))
                    Directory.CreateDirectory(__applictionDirectory + "\\data");

                if (!Directory.Exists(__applictionDirectory + "\\data\\certs"))
                    Directory.CreateDirectory(__applictionDirectory + "\\data\\certs");

                if(!Environment.Is64BitOperatingSystem)
                {
                    if (!File.Exists(__applictionDirectory + "\\libeay32.dll"))
                        File.WriteAllBytes(__applictionDirectory + "\\libeay32.dll", Properties.Resources.libeay32_64);

                    if (!File.Exists(__applictionDirectory + "\\ssleay32.dll"))
                        File.WriteAllBytes(__applictionDirectory + "\\ssleay32.dll", Properties.Resources.ssleay32_64);
                }
                else
                {
                    if (!File.Exists(__applictionDirectory + "\\libeay32.dll"))
                        File.WriteAllBytes(__applictionDirectory + "\\libeay32.dll", Properties.Resources.libeay32_32);

                    if (!File.Exists(__applictionDirectory + "\\ssleay32.dll"))
                        File.WriteAllBytes(__applictionDirectory + "\\ssleay32.dll", Properties.Resources.ssleay32_32);

                    if (!File.Exists(__applictionDirectory + "\\data\\openssl.cnf"))
                        File.WriteAllBytes(__applictionDirectory + "\\data\\openssl.cnf", Properties.Resources.openssl_cnf);
                }
                
                if (!File.Exists(__applictionDirectory + "\\data\\openssl.cnf"))
                    File.WriteAllBytes(__applictionDirectory + "\\data\\openssl.cnf", Properties.Resources.openssl_cnf);

                __userCertName = __applictionDirectory + "\\data\\certs\\" + Environment.UserName.ComputeSHA1();

                //if (!File.Exists(__userCertName + ".crt"))
                //{
                //    var now = DateTime.Now;
                //    var future = now + TimeSpan.FromDays(365 * 5);

                //    using (var cfg = new OpenSSL.X509.Configuration(__applictionDirectory + "\\data\\openssl.cnf"))
                //    using (var subject = new X509Name("CN=nettools Self-Signed Certificate"))
                //    using (var ca = X509CertificateAuthority.SelfSigned(cfg, new SimpleSerialNumber(), "nettools by Lukas Berger", DateTime.Now, TimeSpan.FromDays(365 * 5)))
                //    using (var rsa = new RSA())
                //    {
                //        rsa.GenerateKeys(8192, new BigNumber((uint)3), null, null);
                //        using (var key = new CryptoKey(rsa))
                //        {
                //            var request = new X509Request(1, subject, key);
                //            var cert = ca.ProcessRequest(request, now, future, cfg, "tls_server");
                //            cert.PrivateKey = key;

                //            __userCert = cert;

                //            using (var bio = BIO.MemoryBuffer())
                //            {
                //                string __private = "", __public = "";

                //                rsa.WritePrivateKey(bio, Cipher.AES_256_CBC, (a, b) => { return "\0"; }, __userCertName + ".key");
                //                __private = bio.ReadString();

                //                rsa.WritePublicKey(bio);
                //                __public = bio.ReadString();

                //                File.WriteAllText(__userCertName + ".key", __private);
                //                File.WriteAllText(__userCertName + ".pub", __public);
                //            }

                //            using (var bio = BIO.File(__userCertName + ".crt", "w"))
                //                cert.Write_DER(bio);
                //        }
                //    }
                //}
                //else
                //    using (var priv_bio = BIO.File(__userCertName + ".key", "r"))
                //    using (var bio = BIO.File(__userCertName + ".crt", "r"))
                //    {
                //        __userCert = X509Certificate.FromDER(bio);
                //        __userCert.PrivateKey = CryptoKey.FromPrivateKey(priv_bio, "\0");
                //    }

                List<Assembly> pluginAssemblies = PluginCompiler.CompilePlugins(new DirectoryInfo(__applictionDirectory + "\\plugins").GetFiles());
                PluginLoader.LoadPlugins(pluginAssemblies);
            
                CommandExecutor.LoadCommands();
                
                finished = true;
            }).Start();

            Console.CancelKeyPress += (o, e) =>
            {
                if (__workingThread != null)
                {
                    e.Cancel = true;
                    try
                    {
                        Console.CursorVisible = true;
                        __workingCommand.Stop();
                        __workingThread.Abort();

                        waiter.Set();
                    }
                    catch (ThreadAbortException) { }
                    finally
                    {
                        __workingThread = null;
                    }
                }
				else
                {
                    foreach (IConfiguration config in __pluginsConfiguration.Values)
                        config.Save();

                    Console.Write("\n\n\tGoodbye!\n\n");
                }
            };

            Console.CursorTop = 5;
            Console.CursorLeft = 0;

            int stat = 1;
            while(!finished)
            {
                char status = '\0';

                if (stat == 1)
                    status = '|';
                else if (stat == 2)
                    status = '/';
                else if (stat == 3)
                    status = '-';
                else if (stat == 4)
                {
                    status = '\\';
                    stat = 0;
                }
                stat++;
                
                Console.Write("\r=" + new string(' ', 13) + "Welcome to nettools. " + status + new string(' ', 13) + "=");
                Thread.Sleep(50);
            }

            Console.Clear();
            PrintCopyright(true);

            string startDir = Directory.GetCurrentDirectory();

            Console.CursorVisible = true;

            while (true)
            {
                Console.Write(Directory.GetCurrentDirectory() + "> ");
                int lastTop = Console.CursorTop;
                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                {
                    Console.CursorTop = lastTop;
                    continue;
                }

                Tuple<string, Dictionary<string, string>> commandItem = ArgumentParser.Parse(input);

                if (commandItem.Item1 == "exit")
                {
                    break;
                }
                else if (commandItem.Item1 == "clear")
                {
                    Console.Clear();
                    Console.CursorTop = 0;
                    Console.CursorLeft = 0;
                    PrintCopyright();
                }
                else if (commandItem.Item1 == "help" || commandItem.Item1 == "?")
                {
                    CommandExecutor.GetHelp(input.Contains(" ") ? input.Split(new char[] { ' ' }, 2)[1] : "");
                }
                else if (commandItem.Item1 == "man" || commandItem.Item1 == "manual")
                {
                    CommandExecutor.GetHelp(input.Contains(" ") ? input.Split(new char[] { ' ' }, 2)[1] : "", true);
                    PrintCopyright();
                }
                else if(commandItem.Item1 == "debug")
                {
                    if(__debug)
                    {
                        __debug = false;
                        Logger.Info("Debugging-Mode deactivated!");
                    }
                    else
                    {
                        __debug = true;
                        Logger.Info("Debugging-Mode activated!");
                    }
                }
                else if (commandItem.Item1 == "cd")
                {
                    if(input.Contains(" "))
                    {
                        string oldCD = __currentDirectory.ToString();
                        __currentDirectory = input.Split(new char[] { ' ' }, 2)[1];

                        if(!Directory.Exists(__currentDirectory))
                        {
                            Console.WriteLine("The directory \"" + __currentDirectory + "\" does not exists");
                            __currentDirectory = oldCD;
                        }
                        else
                            Directory.SetCurrentDirectory(__currentDirectory);
                    }
                    else
                    {
                        Directory.SetCurrentDirectory(startDir);
                    }
                }
                else
                {
                    try
                    {
                        Console.WriteLine();
                        
                        __workingThread = new Thread(() => 
                        {
                            __workingCommand = CommandExecutor.ExecuteCommand(commandItem.Item1, commandItem.Item2);
                            if(__workingCommand != null)
                                __workingCommand.Execute(commandItem.Item2);

                            waiter.Set();
                        });
                        __workingThread.Start();

                        waiter.WaitOne();
                        waiter.Reset();
                    }
                    catch (ThreadAbortException) { }
                    catch (Exception ex)
                    {
                        if (__debug)
                            Logger.Exception(ex);
                        else
                            Logger.Error(ex.Message, "Command");
                    }
                }

                Console.WriteLine();
            }

            foreach (IConfiguration config in __pluginsConfiguration.Values)
                config.Save();

            Logger.CloseFile();
        }
                
        public static void PrintCopyright(bool newLineAtEnd = false)
        {
            Console.WriteLine("nettools [Version " + Assembly.GetExecutingAssembly().GetName().Version + "]");
            Console.WriteLine("(c) 2015 Lukas Berger");
            if (newLineAtEnd)
                Console.WriteLine();
        }
        
    }

}
