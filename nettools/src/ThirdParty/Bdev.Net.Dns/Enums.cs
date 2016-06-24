#region
//
// nettools.ThirdParty.Bdev.Net.Dns by Rob Philpott, Big Developments Ltd. Please send all bugs/enhancements to
// rob@bigdevelopments.co.uk  This file and the code contained within is freeware and may be
// distributed and edited without restriction.
// 

#endregion

using System;

namespace nettools.ThirdParty.Bdev.Net.Dns
{
    /// <summary>
    /// The DNS TYPE (RFC1035 3.2.2/3) - 4 types are currently supported. Also, I know that this
    /// enumeration goes against naming guidelines, but I have done this as an ANAME is most
    /// definetely an 'ANAME' and not an 'Aname'
    /// </summary>
    public enum DnsType
	{
		None = 0,

        /// <summary>
        /// a host address
        /// </summary>
        ANAME = 1,

        /// <summary>
        /// an authoritative name server
        /// </summary>
        NS = 2,

        /// <summary>
        /// 
        /// </summary>
        SOA = 6,

        /// <summary>
        /// a well known service description
        /// </summary>
        WKS = 11,

        /// <summary>
        /// a domain name pointer
        /// </summary>
        PTR = 12,

        /// <summary>
        /// host information
        /// </summary>
        HINFO = 13,

        /// <summary>
        /// mailbox or mail list information
        /// </summary>
        MINFO = 14,

        /// <summary>
        /// mail exchange
        /// </summary>
        MX = 15,

        /// <summary>
        /// text strings
        /// </summary>
        TXT = 16,

        /// <summary>
        /// A request for all records
        /// </summary>
        Any = 255
	}

	/// <summary>
	/// The DNS CLASS (RFC1035 3.2.4/5)
	/// Internet will be the one we'll be using (IN), the others are for completeness
	/// </summary>
	public enum DnsClass
	{
		None = 0,

        /// <summary>
        /// the Internet
        /// </summary>
        IN = 1,

        /// <summary>
        /// the CSNET class (Obsolete - used only for examples in some obsolete RFCs)
        /// </summary>
        CS = 2,

        /// <summary>
        /// the CHAOS class
        /// </summary>
        CH = 3,

        /// <summary>
        /// Hesiod [Dyer 87]
        /// </summary>
        HS = 4,

        /// <summary>
        /// any class
        /// </summary>
        Any = 255
	}

	/// <summary>
	/// (RFC1035 4.1.1) These are the return codes the server can send back
	/// </summary>
	public enum ReturnCode
	{
		Success = 0,
		FormatError = 1,
		ServerFailure = 2,
		NameError = 3,
		NotImplemented = 4,
		Refused = 5,
		Other = 6
	}

	/// <summary>
	/// (RFC1035 4.1.1) These are the Query Types which apply to all questions in a request
	/// </summary>
	public enum Opcode
	{
		StandardQuery = 0,
		InverseQuerty = 1,
		StatusRequest = 2,
		Reserverd3 = 3,
		Reserverd4 = 4,
		Reserverd5 = 5,
		Reserverd6 = 6,
		Reserverd7 = 7,
		Reserverd8 = 8,
		Reserverd9 = 9,
		Reserverd10 = 10,
		Reserverd11 = 11,
		Reserverd12 = 12,
		Reserverd13 = 13,
		Reserverd14 = 14,
		Reserverd15 = 15,
	}
}
