using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using nettools.Utils;

namespace nettools.Core.Commands
{

    internal class PortScanCommand : ICommand
    {
        
        public bool Execute(Dictionary<string, string> arguments)
        {
            string hostnameOrIpAddress = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address", "ip");
            ushort startPort = 0;
            ushort endPort = ushort.MaxValue;

            addressFamily = SocketUtils.GetAddressFamilyFromString(ArgumentParser.StripArgument(arguments, "ipv4", "af", "addrfam", "addressfamily"));
            socketType = SocketUtils.GetSocketTypeFromString(ArgumentParser.StripArgument(arguments, "stream", "st", "socktype", "sockettype"));
            protocolType = SocketUtils.GetProtocolTypeFromString(ArgumentParser.StripArgument(arguments, "tcp", "pt", "prottype", "protocoltype"));

            timeout = ArgumentParser.StripArgument(arguments, 500, "t", "timeout");
            int dummy = 0;

            bool isNumeric = false;

            if (arguments.Count > 1)
                isNumeric = int.TryParse(arguments.Keys.ToArray()[1], out dummy);

            if (!isNumeric && !arguments.ContainsKey("s") && !arguments.ContainsKey("start") && !arguments.ContainsKey("p") && !arguments.ContainsKey("port"))
            {
                string definedRange = ArgumentParser.StripArgument(arguments, "", "r", "range");

                if (definedRange == "full")
                {
                    startPort = 0;
                    endPort = ushort.MaxValue;
                }
                else
                {
                    Console.WriteLine("Please enter a valid range");
                    return false;
                }
            }
            else if ((isNumeric && arguments.Count > 2 && !(arguments.ContainsKey("s") && arguments.ContainsKey("start"))) && !arguments.ContainsKey("r") && !arguments.ContainsKey("range"))
            {
                startPort = ArgumentParser.StripArgument<ushort>(arguments, 0, 1, "s", "start");
                endPort = ArgumentParser.StripArgument<ushort>(arguments, ushort.MaxValue, 2, "e", "end");
            }
            else if (isNumeric && !arguments.ContainsKey("s") && !arguments.ContainsKey("start") && !arguments.ContainsKey("r") && !arguments.ContainsKey("range"))
            {
                startPort = ArgumentParser.StripArgument<ushort>(arguments, 0, 1, "p", "port");
                endPort = startPort;
            }            

            if (string.IsNullOrEmpty(hostnameOrIpAddress) || string.IsNullOrWhiteSpace(hostnameOrIpAddress))
            {
                Console.WriteLine("Please enter a hostname of IP-Address");
                return false;
            }

            openPorts = new List<int>();
            consoleLock = new object();
            waitingForResponses = 0;
            maxQueriesAtOneTime = 100;
            stop = false;
            currStatusTop = 0;
            currTableTop = 0;
            currAddress = "";

            addressFamily = AddressFamily.Unspecified;
            socketType = SocketType.Unknown;
            protocolType = ProtocolType.Unspecified;

            StartPortScanning(hostnameOrIpAddress, startPort, endPort);

            /* Console.WriteLine("=== Scan Results: OPEN PORTS ===\n");
            Console.WriteLine(formatter, "Count", "Address", "Port");
            Console.WriteLine(formatter, new string('=', 6), new string('=', 55), new string('=', 6));

            int count = 0;
            foreach (int port in openPorts)
            {
                Console.WriteLine(formatter, count + 1, hostnameOrIpAddress, port);
                count++;
            } */

            return true;
        }

        private List<int> openPorts = new List<int>();
        private object consoleLock = new object();
        private int waitingForResponses = 0;
        private int maxQueriesAtOneTime = 100;
        private int timeout = 1500;
        private bool stop = false;

        private int currStatusTop = 0;
        private int currTableTop = 0;
        private string currAddress = "";

        AddressFamily addressFamily;
        SocketType socketType;
        ProtocolType protocolType;

        private void StartPortScanning(string ipAddress, int startPort, int endPort)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");

            if (!IPAddress.TryParse(ipAddress, out ip))
                ip = Dns.GetHostEntry(ipAddress).AddressList[0];
            
            currStatusTop = Console.CursorTop;
            currAddress = ipAddress;

            string formatter = " {0,-6} | {1,-55} | {2,-6}";

            Console.WriteLine("\n\n=== Scan Results: OPEN PORTS ===\n");
            Console.WriteLine(formatter, "Count", "Address", "Port");
            Console.WriteLine(formatter, new string('=', 6), new string('=', 55), new string('=', 6));
            currTableTop = Console.CursorTop;
            
            for (int i = startPort; i <= endPort; i++)
            {
                lock (consoleLock)
                {
                    Console.CursorTop = currStatusTop;
                    Console.Write("Scanning port: {0}\r", i);
                    Console.CursorLeft = 0;
                }

                while (waitingForResponses >= maxQueriesAtOneTime)
                    Thread.Sleep(0);

                if (stop)
                    break;

                try
                {
                    Socket socket = new Socket(addressFamily, socketType, protocolType);
                    socket.ReceiveTimeout = timeout;
                    IAsyncResult result = socket.BeginConnect(new IPEndPoint(ip, i), EndConnect, socket);
                    
                    bool success = result.AsyncWaitHandle.WaitOne(socket.ReceiveTimeout, true);
                    if (!success)
                    {
                        socket.Close();
                    }
                                        
                    IncrementResponses();
                }
                catch (Exception /* ex */)
                {
                    // Logger.Exception(ex);
                }
            }
        }

        private void EndConnect(IAsyncResult ar)
        {
            try
            {
                DecrementResponses();

                Socket socket = ar.AsyncState as Socket;
                socket.EndConnect(ar);

                if (socket.Connected)
                {
                    int openPort = Convert.ToInt32(socket.RemoteEndPoint.ToString().Split(':')[1]);
                    openPorts.Add(openPort);
                    socket.Close();
                    
                    Console.CursorTop = currTableTop;
                    string formatter = " {0,-6} | {1,-55} | {2,-6}";
                    Console.WriteLine(formatter, openPorts.Count, currAddress, openPort);
                    currTableTop++;
                }
            }
            catch (Exception) { }
        }
        
        private void IncrementResponses()
        {
            Interlocked.Increment(ref waitingForResponses);
        }

        private void DecrementResponses()
        {
            Interlocked.Decrement(ref waitingForResponses);
        }
        
        public string[] GetAvailableCommands()
        {
            return new string[] { "portscan", "scanports", "pscan" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "a*0", "addr|a", "address|a", "ip|a", "r*1", "range|r", "p", "port|p", "s*2", "start|s", "e*2", "end|e", "t", "timeout|t",
                "st", "stype|st", "sckttype|st", "sockettype|st" };
        }

        public string GetHelp()
        {
            return "Running a Port-Scan on the given server";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "a")
                return "The hostname/IP-Address which should be scanned";
            else if (arg == "r")
                return "Scan ports with a pre-defined Range (Available: full)";
            else if (arg == "p")
                return "Port to scan (When p is set, only the given port will be scanned)";
            else if (arg == "s")
                return "The start-port of the Scan-Range";
            else if (arg == "e")
                return "The end-port of the Scan-Range";
            else if (arg == "t")
                return "Time in milliseconds after which the connection should close";
            else
                return null;
        }

        public void Stop() { }

    }

}
