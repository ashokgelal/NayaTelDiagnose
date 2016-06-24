using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

using nettools.Utils;

namespace nettools.Core.Commands
{

    internal class TelnetCommand : ICommand
    {

        private Socket client;
        private ManualResetEvent waiter = new ManualResetEvent(false);

        public bool Execute(Dictionary<string, string> arguments)
        {
            string address = ArgumentParser.StripArgument(arguments, "", 0, "a", "addr", "address", "host", "hostname");
            ushort port = ArgumentParser.StripArgument<ushort>(arguments, 23, 1, "p", "port");

            AddressFamily addressFamily = SocketUtils.GetAddressFamilyFromString(ArgumentParser.StripArgument(arguments, "ipv4", "af", "addrfam", "addressfamily"));
            SocketType socketType = SocketUtils.GetSocketTypeFromString(ArgumentParser.StripArgument(arguments, "stream", "st", "socktype", "sockettype"));
            ProtocolType protocolType = SocketUtils.GetProtocolTypeFromString(ArgumentParser.StripArgument(arguments, "tcp", "pt", "prottype", "protocoltype"));

            if (string.IsNullOrEmpty(address) || string.IsNullOrWhiteSpace(address))
            {
                Console.WriteLine("Please enter an adress to which should be connected");
                return false;
            }

            client = new Socket(addressFamily, socketType, protocolType);
            client.Connect(address, port);

            long totalRead = 0;

            Thread readThrd = new Thread(() =>
            {
                while (client.Connected)
                {
                    if (client.Available > 0)
                    {
                        byte[] data = new byte[client.Available];
                        totalRead += client.Read(data, 0, data.Length, SocketFlags.None);

                        for (int i = 0; i < data.Length; i++)
                        {
                            char c = (char)data[i];

                            if (c == '\n')
                            {
                                Console.CursorTop++;
                                Console.CursorLeft = 0;
                            }
                            else if (c == '\r')
                            {
                                Console.CursorLeft = 0;
                            }
                            else
                            {
                                Console.Write(c);
                            }
                        }
                    }
                }
            });
            
            Thread writeThrd = new Thread(() =>
            {
                while (client.Connected)
                {
                    ConsoleKeyInfo inf = Console.ReadKey();
                    char c = inf.KeyChar;

                    if (inf.Key == ConsoleKey.Backspace)
                    {
                        client.Send(new byte[] { 0x08, 0x20, 0x08 });
                        Console.Write(' ');
                        if (Console.CursorLeft != 1)
                            Console.CursorLeft--;
                    }
                    else if (c == '\r')
                    {
                        client.Write("\r\n");
                        Console.CursorTop++;
                    }
                    else
                    {
                        client.Write(c);
                    }
                }
            });

            Logger.Debug("Starting read-thread for telnet-connection...");
            readThrd.Start();
            Logger.Debug("Starting writing-thread for telnet-connection...");
            writeThrd.Start();

            waiter.WaitOne();
            waiter.Reset();

            Logger.Debug("Stopping read-thread...");
            readThrd.Abort();
            Logger.Debug("Stopping writing-thread...");
            writeThrd.Abort();
            
            return true;
        }
        
        public string[] GetAvailableCommands()
        {
            return new string[] { "telnet", "teln" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "a*0", "addr|a", "address|a", "host|a", "hostname|a", "p*1", "port|p" };
        }

        public string GetHelp()
        {
            return "Wake up the device with the given MAC-Address";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "m")
                return "The MAC-Address of the device which should be woken up";
            else
                return null;
        }

        public void Stop()
        {
            client.Disconnect(false);
            waiter.Set();
        }

    }

}
