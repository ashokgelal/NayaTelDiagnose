using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace nettools.Utils
{

    internal static class IPUtils
	{

		public static IEnumerable<IPAddress> GetIPRange(IPAddress startIP, IPAddress endIP)
		{
			uint sIP = IPAddressToUInt(startIP.GetAddressBytes());
			uint eIP = IPAddressToUInt(endIP.GetAddressBytes());
			while (sIP <= eIP)
			{
				yield return new IPAddress(ReverseBytesArray(sIP));
				sIP++;
			}
		}

		public static uint ReverseBytesArray(uint ip)
		{
			byte[] bytes = BitConverter.GetBytes(ip);
			bytes = bytes.Reverse().ToArray();
			return (uint)BitConverter.ToInt32(bytes, 0);
		}

		public static uint IPAddressToUInt(byte[] ipBytes)
		{
			ByteConverter bConvert = new ByteConverter();
			uint ipUint = 0;

			int shift = 24;
			foreach (byte b in ipBytes)
			{
				if (ipUint == 0)
				{
					ipUint = (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
					shift -= 8;
					continue;
				}

				if (shift >= 8)
					ipUint += (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
				else
					ipUint += (uint)bConvert.ConvertTo(b, typeof(uint));

				shift -= 8;
			}

			return ipUint;
		}
		
	}

}
