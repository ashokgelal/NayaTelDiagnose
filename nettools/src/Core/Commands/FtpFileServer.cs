using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using OpenSSL.SSL;

using MimeTypes;

namespace nettools.Core.Commands
{

    internal class FtpFileServer : ICommand
    {

        private TcpListener ftp_server;
        private int connId = 0;
        private ManualResetEvent waiter = new ManualResetEvent(false);

        private string login_user = "", login_password = "";

        private List<int> passiveUsage = new List<int>();
        private int passiv_begin, passiv_end;

        private Random rand = new Random();
        
        public bool Execute(Dictionary<string, string> arguments)
        {
            string bindIP = ArgumentParser.StripArgument(arguments, "127.0.0.1", 0, "ip", "a", "addr", "address");
            int bindPort = ArgumentParser.StripArgument(arguments, 21, 1, "p", "port");
            login_user = ArgumentParser.StripArgument(arguments, "admin", 2, "u", "user");
            login_password = ArgumentParser.StripArgument(arguments, "", 3, "pw", "pass", "password");

            passiv_begin = ArgumentParser.StripArgument(arguments, 9595, "psvb");
            passiv_end = ArgumentParser.StripArgument(arguments, 10595, "psve");

            if(passiv_end <= passiv_begin)
            {
                Logger.Info("\"psve\" cannot be less than or equal to \"psvb\"");
                return false;
            }

            if (passiv_begin < 1024)
            {
                Logger.Info("\"psvb\" have to be greater than 1024");
                return false;
            }

            passiveUsage.Clear();

            Logger.Info("Starting FTP-Server on " + bindIP + ":" + bindPort + "...");
            ftp_server = new TcpListener(IPAddress.Parse(bindIP), bindPort);
            ftp_server.Start();
            ftp_server.BeginAcceptTcpClient(OnAcceptClient, ftp_server);

            if (ftp_server.Server.IsBound)
                Logger.Info("FTP-Server running!");
            else
                Logger.Error("Failed to start FTP-Server");

            waiter.WaitOne();
            waiter.Reset();
            connId = 0;

            return true;
        }

        #region Connection-Handler

        private void OnAcceptClient(IAsyncResult ar)
        {
            int currConnectionId = connId++;
            TcpClient client = null;

            try
            {
                TcpListener server = (TcpListener)ar.AsyncState;
                client = server.EndAcceptTcpClient(ar);
                IPEndPoint clientRemote = (IPEndPoint)client.Client.RemoteEndPoint;

                Logger.Info("Incoming Connection from " + clientRemote.Address + ":" + clientRemote.Port, currConnectionId.ToString());

                if (server.Server.IsBound)
                    server.BeginAcceptTcpClient(OnAcceptClient, server);

                Stream stream = new NetworkStream(client.Client);
                ProcessConection(client, stream, currConnectionId);
            }
            catch (ObjectDisposedException)
            {
                if (client != null)
                    client.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occured: " + ex.Message, currConnectionId.ToString());
                client.Close();
            }
        }
        
        private void ProcessConection(TcpClient client, Stream stream, int currConnectionId)
        {
            IPEndPoint clientRemote = (IPEndPoint)client.Client.RemoteEndPoint;

            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);

            writer.AutoFlush = false;
            writer.NewLine = "\r\n";

            writer.WriteLine("220 nettools FTP-Fileserver ready!");
            writer.Flush();

            string __ftp_login_user = "", __ftp_login_pass = "", __ftp_curr_dir = "/";
            bool __logged_in = false, __use_tls = false, __user_sent = false;

            string __ftp_data_conn_type = "";

            TcpListener __passive_srv = null;
            TcpClient __data_transfer_client = null;
            Stream __data_transfer_stream = null;
            StreamWriter __data_transfer_writer = null;
            StreamReader __data_transfer_reader = null;

            while (client.Connected)
            {
                Tuple<string, string> command = ParseFTPCommand(reader.ReadLine());

                if (command.Item1 == null) // I don't like null-packets
                {
                    client.Close();
                    return;
                }

                Logger.Info("Command: " + command.Item1 + "; Arguments: " + command.Item2, currConnectionId.ToString());

                switch (command.Item1)
                {
                    case "USER":
                        __user_sent = true;
                        __ftp_login_user = command.Item2;
                        if (login_password != "")
                            SendMessage(writer, 331, "Password required for " + __ftp_login_user);
                        else
                        {
                            SendMessage(writer, 320, "User " + __ftp_login_user + " logged in");
                            __logged_in = true;
                        }
                        break;

                    case "PASS":
                        if (!__user_sent)
                            SendMessage(writer, 332, "Send your username first");
                        else
                        {
                            __ftp_login_pass = command.Item2;
                            if (__ftp_login_user == login_user || __ftp_login_pass != login_password)
                            {
                                SendMessage(writer, 430, "Invalid username and/or password");
                            }
                            else
                            {
                                SendMessage(writer, 320, "User " + __ftp_login_user + " logged in");
                                __logged_in = true;
                            }
                        }
                        break;

                    case "AUTH":
                        
                        if(command.Item2.ToUpper() == "TLS")
                        {
                            SendMessage(writer, 234, "Enabling TLS Connection");
                            __use_tls = true;

                            SslStream sslStream = new SslStream(stream, true);
                            sslStream.AuthenticateAsServer(Program.__userCert, false, null, SslProtocols.Tls, SslStrength.All, false);

                            reader = new StreamReader(sslStream);
                            writer = new StreamWriter(sslStream);
                        }
                        else
                        {
                            SendMessage(writer, 504, "504 Unrecognized AUTH mode");
                        }

                        break;

                    case "PWD":

                        if (!__logged_in)
                        {
                            SendMessage(writer, 230, "Not logged in");
                            break;
                        }

                        SendMessage(writer, 257, "\"" + __ftp_curr_dir + "\" is current directory.");
                        break;

                    case "CWD":

                        if (!__logged_in)
                        {
                            SendMessage(writer, 230, "Not logged in");
                            break;
                        }

                        __ftp_curr_dir = ChangeWorkingDirectory(command.Item2, __ftp_curr_dir, writer);
                        break;

                    case "CDUP":

                        if (!__logged_in)
                        {
                            SendMessage(writer, 230, "Not logged in");
                            break;
                        }

                        __ftp_curr_dir = ChangeWorkingDirectory("..", __ftp_curr_dir, writer);
                        break;

                    case "SYST":

                        if (!__logged_in)
                        {
                            SendMessage(writer, 230, "Not logged in");
                            break;
                        }

                        SendMessage(writer, 215, "UNIX Type: L8"); // Simulating unix, e.g. paths
                        break;

                    case "TYPE":

                        if (!__logged_in)
                        {
                            SendMessage(writer, 230, "Not logged in");
                            break;
                        }

                        string[] type_args = command.Item2.Split(' ');

                        switch(type_args[0])
                        {
                            case "A":
                            case "I":
                                __ftp_data_conn_type = type_args[0];
                                SendMessage(writer, 200, "OK");
                                break;

                            case "E":
                            case "L":
                            default:
                                SendMessage(writer, 504, "Command not implemented for that parameter.");
                                break;
                        }

                        if(type_args.Length > 1)
                        {
                            switch (type_args[1])
                            {
                                case "N":
                                    SendMessage(writer, 200, "OK");
                                    break;

                                case "T":
                                case "C":
                                default:
                                    SendMessage(writer, 504, "Command not implemented for that parameter.");
                                    break;
                            }
                            
                        }

                        break;

                    case "PASV":

                        if (!__logged_in)
                        {
                            SendMessage(writer, 230, "Not logged in");
                            break;
                        }
                        
                        if (passiveUsage.Count == passiv_begin - passiv_end)
                        {
                            SendMessage(writer, 425, "Cannot open data connection, no available port for passive connections");
                        }
                        else
                        {
                            string[] addressParts = ((IPEndPoint)ftp_server.Server.LocalEndPoint).Address.ToString().Split('.');
                            int randPort = rand.Next(passiv_begin, passiv_end);

                            while (passiveUsage.Contains(randPort))
                                randPort = rand.Next(passiv_begin, passiv_end);
                            
                            byte[] portArray = BitConverter.GetBytes(randPort);

                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(portArray);

                            if(__passive_srv != null)
                            {
                                __passive_srv.Stop();
                                __passive_srv = null;
                            }

                            passiveUsage.Add(randPort);

                            if (__data_transfer_client != null && __data_transfer_client.Connected)
                            {
                                __data_transfer_client.Close();
                                __data_transfer_client = null;
                            }

                            __passive_srv = new TcpListener(((IPEndPoint)ftp_server.Server.LocalEndPoint).Address, randPort);
                            __passive_srv.Start();

                            SendMessage(writer, 227, string.Format("Entering Passive Mode ({0},{1},{2},{3},{4},{5})",
                                addressParts[0], addressParts[1], addressParts[2], addressParts[3], portArray[2], portArray[3]));
                        }

                        break;

                    case "PORT":

                        if (!__logged_in)
                        {
                            SendMessage(writer, 230, "Not logged in");
                            break;
                        }

                        string[] activeAddress = command.Item2.Split(',');

                        string ipaddr = activeAddress[0] + "." + activeAddress[1] + "." + activeAddress[2] + "." + activeAddress[3];
                        int port = int.Parse(activeAddress[4]) * 256 + int.Parse(activeAddress[5]);

                        if(__passive_srv != null)
                        {
                            __passive_srv.Stop();
                            __passive_srv = null;
                        }

                        __data_transfer_client = new TcpClient();
                        __data_transfer_client.Connect(ipaddr, port);

                        SendMessage(writer, 200, "PORT command successful");

                        break;

                    case "LIST":

                        if (!__logged_in)
                        {
                            SendMessage(writer, 230, "Not logged in");
                            break;
                        }

                        SendMessage(writer, 150, "Opening " + __ftp_data_conn_type + " mode data transfer for LIST");

                        if(__passive_srv != null && __passive_srv.Server.IsBound)
                            __data_transfer_client = __passive_srv.AcceptTcpClient();
    
                        __data_transfer_stream = __data_transfer_client.GetStream();

                        if (__use_tls)
                        {
                            SslStream sslStream = new SslStream(__data_transfer_stream, true);
                            sslStream.AuthenticateAsServer(Program.__userCert, false, null, SslProtocols.Tls, SslStrength.High, false);

                            __data_transfer_writer = new StreamWriter(sslStream);
                            __data_transfer_reader = new StreamReader(sslStream);
                        }
                        else
                        {
                            __data_transfer_writer = new StreamWriter(__data_transfer_stream);
                            __data_transfer_reader = new StreamReader(__data_transfer_stream);
                        }

                        foreach (var d in new DirectoryInfo(Program.CurrentDirectory.TrimEnd('\\') + "\\" + __ftp_curr_dir.Replace('/', '\\').TrimEnd('\\')).GetDirectories())
                        {
                            string date = d.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180) ?
                                d.LastWriteTime.ToString("MMM dd  yyyy") :
                                d.LastWriteTime.ToString("MMM dd HH:mm");

                            string line = string.Format("drwxr-xr-x    2 2003     2003     {0,8} {1} {2}", "4096", date, d.Name);

                            __data_transfer_writer.WriteLine(line);
                            __data_transfer_writer.Flush();
                        }

                        foreach (var f in new DirectoryInfo(Program.CurrentDirectory.TrimEnd('\\') + "\\" + __ftp_curr_dir.Replace('/', '\\').TrimEnd('\\')).GetFiles())
                        {
                            string date = f.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180) ?
                                f.LastWriteTime.ToString("MMM dd  yyyy") :
                                f.LastWriteTime.ToString("MMM dd HH:mm");

                            string line = string.Format("-rw-r--r--    2 2003     2003     {0,8} {1} {2}", f.Length, date, f.Name);

                            __data_transfer_writer.WriteLine(line);
                            __data_transfer_writer.Flush();
                        }

                        __data_transfer_writer.Flush();
                        __data_transfer_client.Close();
                        __data_transfer_client = null;
                        
                        __data_transfer_stream.Close();
                        __data_transfer_stream.Dispose();

                        __data_transfer_writer.Close();
                        __data_transfer_writer.Dispose();

                        __data_transfer_reader.Close();
                        __data_transfer_reader.Dispose();

                        if (__passive_srv != null)
                        {
                            __passive_srv.Stop();
                            passiveUsage.Remove(((IPEndPoint)__passive_srv.LocalEndpoint).Port);
                        }

                        writer.WriteLine("226 Transfer complete");
                        writer.Flush();

                        break;

                    case "RETR":

                        if(!__logged_in)
                        {
                            SendMessage(writer, 230, "Not logged in");
                            break;
                        }

                        string curr_path = (Program.CurrentDirectory.TrimEnd('\\') + "\\" + __ftp_curr_dir.Replace('/', '\\').TrimEnd('\\'));
                        FileInfo file = new FileInfo(curr_path + "/" + Path.GetFileName(command.Item2));

                        SendMessage(writer, 150, "Opening " + __ftp_data_conn_type + " mode data transfer for '" + file + "'");

                        if (__passive_srv != null && __passive_srv.Server.IsBound)
                            __data_transfer_client = __passive_srv.AcceptTcpClient();

                        __data_transfer_stream = __data_transfer_client.GetStream();
                        __data_transfer_writer = new StreamWriter(__data_transfer_stream);
                        __data_transfer_reader = new StreamReader(__data_transfer_stream);
                        BinaryWriter __data_binary_writer = new BinaryWriter(__data_transfer_stream);

                        FileStream fileReader = file.OpenRead();
                        byte[] buffer = new byte[1024];
                        long current = 0, total = 0;

                        total = fileReader.Length;

                        do
                        {
                            int read = fileReader.Read(buffer, 0, buffer.Length);
                            current += read;
                            __data_binary_writer.Write(buffer, 0, read);
                            __data_binary_writer.Flush();
                        } while (current < total);

                        __data_binary_writer.Flush();
                        __data_binary_writer.Close();
                        __data_binary_writer.Dispose();

                        __data_transfer_writer.Close();
                        __data_transfer_writer.Dispose();

                        __data_transfer_reader.Close();
                        __data_transfer_reader.Dispose();

                        __data_transfer_stream.Close();
                        __data_transfer_stream.Dispose();
                        
                        if (__passive_srv != null)
                        {
                            __passive_srv.Stop();
                            passiveUsage.Remove(((IPEndPoint)__passive_srv.LocalEndpoint).Port);
                        }
                        
                        __data_transfer_client.Close();
                        __data_transfer_client = null;

                        writer.WriteLine("226 Transfer complete");
                        writer.Flush();

                        break;

                    case "QUIT":
                        SendMessage(writer, 221, "Service closing control connection");
                        client.Close();
                        break;

                    default:
                        SendMessage(writer, 502, "Command not implemented");
                        break;
                }
            }

            client.Close();
            Logger.Info("Connection closed.", currConnectionId.ToString());
        }

        #region Command-Executors

        private string ChangeWorkingDirectory(string new_path, string curr_path, StreamWriter writer)
        {
            DirectoryInfo allowed = new DirectoryInfo(Program.CurrentDirectory);
            string curr_str = "";

            if (new_path.StartsWith("/")) // navigate from root
                curr_str = Program.CurrentDirectory.TrimEnd('\\') + "\\" + new_path.Replace('/', '\\');
            else // navigate from current
                curr_str = (Program.CurrentDirectory.TrimEnd('\\') + "\\" + curr_path.Replace('/', '\\').TrimEnd('\\')).TrimEnd('\\') +
                    "\\" + new_path.Replace('/', '\\');

            DirectoryInfo curr = new DirectoryInfo(curr_str);

            if (IsCurrentDirectoryOpen(allowed.FullName, curr.FullName))
            {
                SendMessage(writer, 250, "CWD command successful.");
                return '/' + (Program.CurrentDirectory.TrimEnd('\\') != curr.FullName.TrimEnd('\\') ?
                    curr.FullName.Remove(0, Program.CurrentDirectory.Length + 1) : "").Replace('\\', '/');
            }
            else
            {
                SendMessage(writer, 550, "Access to this directory is not allowed");
                return curr_path;
            }
        }

        #endregion

        #region Utilities

        private bool IsCurrentDirectoryOpen(string allowed, string current)
        {
            bool isSame = allowed.TrimEnd('\\') == current;
            bool isCurrentChildren = current.StartsWith(allowed);

            return isSame || isCurrentChildren;
        }

        private void SendMessage(StreamWriter wr, int code, string text)
        {
            wr.WriteLine(/* "\0" + */code + " " + text.TrimEnd('.'));
            wr.Flush();
        }

        private Tuple<string, string> ParseFTPCommand(string line)
        {
            if (line == null)
                return new Tuple<string, string>(null, null);

            string[] parts = line.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            return new Tuple<string, string>(parts[0].ToUpper(), (parts.Length == 2 ? parts[1] : ""));
        }

        #endregion

        #endregion

        public string[] GetAvailableCommands()
        {
            return new string[] { "ftpfs", "ftpserver", "ftpfileserver" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "ip*0", "a|ip", "addr|ip", "address|ip", "p*1", "port|p", "u*2", "user|", "pw*3", "pass|pw", "password|pw", "psvb", "psve" };
        }

        public string GetHelp()
        {
            return "Browsing the current-directory over FTP(ES)";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "ip")
                return "The IP to which the server should be bound";
            else if (arg == "p")
                return "The port of the FTP-Server";
            else if (arg == "u")
                return "The username which should be required to log in";
            else if (arg == "pw")
                return "The password which should be required to log in";
            else if (arg == "psvb")
                return "The begin of the range of passive ports";
            else if (arg == "psve")
                return "The (excluded) end of the range of passive ports";
            else
                return null;
        }

        public void Stop()
        {
            ftp_server.Stop();
            waiter.Set();
        }

    }

}
