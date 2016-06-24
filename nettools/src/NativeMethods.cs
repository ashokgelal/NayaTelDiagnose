using System;
using System.Net;
using System.Runtime.InteropServices;

namespace nettools
{

	internal static class NativeMethods
	{
		
        /// <summary>
        /// TODO: Create replacement for sending ARP's without using WinAPI for full Mono-Compatibility
        /// <returns></returns>
		[DllImport("iphlpapi.dll", ExactSpelling = true)]
		public static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);

	}

}
