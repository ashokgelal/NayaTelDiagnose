using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using nettools.Utils.Net.Headers;
using System.IO;

namespace nettools.Core.Commands
{

    internal class NetworkSnifferCommand : ICommand
    {

        Socket mainSocket;

        private byte[] byteData = new byte[81920];

        private string formatter = " {0,-7} | {1,-9} | {2,-14} | {3,-40} | {4,-40} | {5,-60}";

        private bool running = true;
        private bool printingData = true;
        private string inputedData = "";
        private int dataCount = 0;
        private bool resolveHost = true;

        private Dictionary<int, IPHeader> headerStorage = new Dictionary<int, IPHeader>();
        private Dictionary<IPAddress, string> resolveStorage = new Dictionary<IPAddress, string>();

        public bool Execute(Dictionary<string, string> arguments)
        {
            string ipAddress = ArgumentParser.StripArgument(arguments, "127.0.0.1", 0, "a", "addr", "address");
            resolveHost = ArgumentParser.StripArgument(arguments, true, "r", "reslv", "resolve");

            // For sniffing the socket to capture the packets 
            // has to be a raw socket, with the address family
            // being of type internetwork, and protocol being IP
            mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw,
                                   ProtocolType.IP);

            // Bind the socket to the selected IP address
            mainSocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), 0));

            // Set the socket options
            mainSocket.SetSocketOption(SocketOptionLevel.IP,  //Applies only to IP packets
                                       SocketOptionName.HeaderIncluded, //Set the include header
                                       true);                           //option to true

            byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] byOut = new byte[4];

            //Socket.IOControl is analogous to the WSAIoctl method of Winsock 2
            mainSocket.IOControl(IOControlCode.ReceiveAll,  //SIO_RCVALL of Winsock
                                 byTrue, byOut);

            Console.WriteLine(formatter, "#", "Protocol", "ACK", "Source", "Destination", "Informations");
            Console.WriteLine(formatter, new string('=', 7), new string('=', 9), new string('=', 14), new string('=', 40), new string('=', 40), new string('=', 60));

            //Start receiving the packets asynchronously
            mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                                    new AsyncCallback(OnReceive), null);

            while (running)
            {
                if(!printingData)
                    Console.Write("\r> " + inputedData);

                char key = Console.ReadKey().KeyChar;

                if (key == '\r')
                {
                    Console.WriteLine();
                    if (inputedData == "out")
                    {
                        printingData = !printingData;
                        if (printingData)
                            Console.WriteLine("Output enabled!");
                        else
                            Console.WriteLine("Output disabled!");
                    }
                    else if (inputedData == "stop")
                    {
                        running = false;
                        Console.WriteLine("\n");
                    }
                    else if (inputedData.StartsWith("data "))
                    {
                        string idStr = inputedData.Substring(5);
                        int id = 0;
                        
                        if(int.TryParse(idStr, out id))
                        {
                            if(!headerStorage.ContainsKey(id))
                            {
                                Console.WriteLine("Unknown ID.\tUsage: data [Numeric ID of the packet]");
                            }
                            else
                            {
                                IPHeader header = headerStorage[id];
                                byte[] data = header.Data;

                                switch (header.ProtocolType)
                                {
                                    case Protocol.TCP:
                                        data = new TCPHeader(data, header.MessageLength).Data;
                                        break;

                                    case Protocol.UDP:
                                        data = new UDPHeader(data, header.MessageLength).Data;
                                        break;
                                }

                                int lineCount = 0;
                                int maxLineCount = 25;

                                byte[] dataCount = new byte[maxLineCount];

                                int dataTrimming = data.Length - 1;
                                while (dataTrimming > 0 && data[dataTrimming] == 0)
                                    dataTrimming--;

                                for (int i = 0; i < dataTrimming; i++)
                                {
                                    /* if (data[i] == '\0')
                                        continue; */

                                    dataCount[lineCount] = data[i];
                                    char c = System.Text.Encoding.UTF8.GetString(new byte[] { data[i] })[0];
                                    
                                    Console.Write(data[i].ToString("X2") + " ");
                                    
                                    lineCount++;

                                    if (lineCount == maxLineCount - 1)
                                    {
                                        Console.Write("\t" + System.Text.Encoding.UTF8.GetString(dataCount)
                                            .Replace("\n", " ").Replace("\t", " ").Replace("\r", " ").TrimEnd('\0') + "\n");

                                        lineCount = 0;
                                        dataCount = new byte[maxLineCount];
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid ID.\tUsage: data [Numeric ID of the packet]");
                        }
                    }
                    Console.WriteLine();
                    inputedData = "";
                }
                else if(key == '\b')
                {
                    if(Console.CursorLeft != 1)
                    {
                        Console.Write(" ");
                        Console.CursorLeft--;
                        inputedData = inputedData.Substring(0, inputedData.Length - 1);
                    }
                }
                else
                    inputedData += key;
            }

            return true;
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int nReceived = mainSocket.EndReceive(ar);

                IPHeader ipHeader = new IPHeader(byteData, nReceived);
                headerStorage.Add(dataCount, ipHeader);

                //Analyze the bytes received...
                if (printingData)
                    ParseData(ipHeader);

                dataCount++;

                byteData = new byte[81920];

                //Another call to BeginReceive so that we continue to receive the incoming
                //packets
                if(running)
                    mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), null);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void ParseData(IPHeader ipHeader)
        {
            //Now according to the protocol being carried by the IP datagram we parse 
            //the data field of the datagram

            IPAddress srcIP = ipHeader.SourceAddress;
            IPAddress dstIP = ipHeader.DestinationAddress;

            string src = srcIP.ToString();
            string dst = dstIP.ToString();
            
            if (resolveHost)
            {
                if (resolveStorage.ContainsKey(srcIP))
                    src = resolveStorage[srcIP];
                else
                {
                    resolveStorage.Add(srcIP, src);
                    Dns.BeginGetHostEntry(srcIP, (result) =>
                    {
                        try
                        {
                            IPHostEntry entry = Dns.EndGetHostEntry(result);

                            if (resolveStorage.ContainsKey(srcIP))
                                resolveStorage.Remove(srcIP);
                            resolveStorage.Add(srcIP, entry.HostName);
                        }
                        catch (Exception) { }
                    },
                    null);
                }

                if (resolveStorage.ContainsKey(dstIP))
                    dst = resolveStorage[dstIP];
                else
                {
                    resolveStorage.Add(dstIP, dst);
                    Dns.BeginGetHostEntry(dstIP, (result) =>
                    {
                        try
                        {
                            IPHostEntry entry = Dns.EndGetHostEntry(result);

                            if (resolveStorage.ContainsKey(dstIP))
                                resolveStorage.Remove(dstIP);
                            resolveStorage.Add(dstIP, entry.HostName);
                        }
                        catch (Exception) { }
                    },
                    null);
                }
            }

            switch (ipHeader.ProtocolType)
            {
                case Protocol.TCP:

                    TCPHeader tcpHeader = new TCPHeader(ipHeader.Data, //IPHeader.Data stores the data being carried by the IP datagram
                                                        ipHeader.MessageLength); //Length of the data field         

                    Console.WriteLine("\r" + formatter, dataCount, "TCP", tcpHeader.AcknowledgementNumber,
                        src + ":" + tcpHeader.SourcePort, dst + ":" + tcpHeader.DestinationPort, 
                        "Head: " + tcpHeader.HeaderLength + ", Len: " + tcpHeader.MessageLength + ", Seq: " + tcpHeader.SequenceNumber + 
                        ", Check: " + tcpHeader.Checksum + ", Flags: " + tcpHeader.Flags);

                    Console.Write("> " + inputedData);

                    break;

                case Protocol.UDP:

                    UDPHeader udpHeader = new UDPHeader(ipHeader.Data, //IPHeader.Data stores the data being carried by the IP datagram
                                                       (int)ipHeader.MessageLength); //Length of the data field                    

                    Console.WriteLine("\r" + formatter, dataCount, "UDP", "",
                        src + ":" + udpHeader.SourcePort, dst + ":" + udpHeader.DestinationPort,
                        "Check: " + udpHeader.Checksum);

                    Console.Write("> " + inputedData);

                    break;

                case Protocol.Unknown:
                    break;
            }
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "networksniffer", "netsniffer", "netsniff" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "a*0", "addr|a", "address|a", "r", "reslv|r", "resolve|r" };
        }

        public string GetHelp()
        {
            return "Scans every in- and outgoing traffic to and from this pc";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "a")
                return "The ip of the (local) network-interface which should be monitored";
            else if(arg == "r")
                return "Indicates if the hostname of each IP should be resolved";
            else
                return null;
        }

        public void Stop()
        {
            mainSocket.Disconnect(false);
        }

    }

}