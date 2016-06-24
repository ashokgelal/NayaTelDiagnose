using System;
using System.Net.Sockets;
using System.Text;

namespace nettools.Utils
{

    internal static class SocketUtils
    {

        public static void Write(this Socket socket, string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            socket.Write(data, 0, data.Length);
        }

        public static void Write(this Socket socket, char c)
        {
            socket.Write(c.ToString());
        }
        
        public static void Write(this Socket socket, long l)
        {
            socket.Write(l.ToString());
        }

        public static void WriteLine(this Socket socket, string text)
        {
            socket.Write(text + "\r\n");
        }

        public static void Write(this Socket socket, byte[] data, int offset, int length)
        {
            if (!socket.Connected)
                return;

            socket.Write(data, offset, length, SocketFlags.None);
        }
        
        public static void Write(this Socket socket, byte[] data, int offset, int length, SocketFlags flags)
        {
            if (!socket.Connected)
                return;

            try
            {
                socket.Send(data, offset, length, flags);
            }
            catch (Exception)
            {
                if (Program.__debug)
                    throw;
            }
        }

        public static int Read(this Socket socket, byte[] data, int offset, int length, SocketFlags flags)
        {
            if (!socket.Connected)
                return 0;

            return socket.Receive(data, offset, length, flags);
        }

        public static AddressFamily GetAddressFamilyFromString(string str)
        {
            string _str = str.ToLower();

            if (_str == "ipv4" || _str == "ip4" || _str == "v4" || _str == "internetwork")
                return AddressFamily.InterNetwork;
            else if (_str == "ipv6" || _str == "ip6" || _str == "v6" || _str == "internetworkv6")
                return AddressFamily.InterNetworkV6;
            else if (_str == "unix")
                return AddressFamily.Unix;
            else if (_str == "netbios" || _str == "netbs")
                return AddressFamily.NetBios;
            else 
                return AddressFamily.Unspecified;
        }

        public static SocketType GetSocketTypeFromString(string str)
        {
            string _str = str.ToLower();

            if (_str == "str" || _str == "stream")
                return SocketType.Stream;
            else if (_str == "raw")
                return SocketType.Raw;
            else if (_str == "dg" || _str == "dgr" || _str == "dgrm" || _str == "dgram")
                return SocketType.Dgram;
            else if (_str == "rdm")
                return SocketType.Rdm;
            else if (_str == "seq" || _str == "seqpack" || _str == "seqpacket")
                return SocketType.Seqpacket;
            else
                return SocketType.Unknown;
        }

        public static ProtocolType GetProtocolTypeFromString(string str)
        {
            string _str = str.ToLower();

            if (_str == "tcp")
                return ProtocolType.Tcp;
            else if (_str == "udp")
                return ProtocolType.Udp;
            else if (_str == "raw")
                return ProtocolType.Raw;
            else if (_str == "ip")
                return ProtocolType.IP;
            else if (_str == "icmp4" || _str == "icmpv4" || _str == "icmp")
                return ProtocolType.Icmp;
            else if (_str == "icmp6" || _str == "icmpv6")
                return ProtocolType.IcmpV6;
            else if (_str == "ip4" || _str == "ipv4")
                return ProtocolType.IPv4;
            else if (_str == "ip6" || _str == "ipv6")
                return ProtocolType.IPv6;
            else
                return ProtocolType.Unspecified;
        }

    }

}
