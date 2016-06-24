using System;

namespace nettools.ThirdParty.Bdev.Net.Dns
{
    /// <summary>
    /// An TXT Resource Record (RR) (RFC1035 3.3.14)
    /// </summary>
    public class TXTRecord : RecordBase
    {
        // these fields constitute an SOA RR
        private readonly string _txtData;

        // expose these fields public read/only
        public string TxtData
        {
            get
            {
                return _txtData;
            }
        }

        /// <summary>
        /// Constructs an SOA record by reading bytes from a return message
        /// </summary>
        /// <param name="pointer">A logical pointer to the bytes holding the record</param>
        internal TXTRecord(Pointer pointer)
        {
            // read all fields RFC1035 3.3.13
            _txtData = pointer.ReadString();
        }

        public override string ToString()
        {
            return string.Format("{0}", TxtData);
        }
    }
}
