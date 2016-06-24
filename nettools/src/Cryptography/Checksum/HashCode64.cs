using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nettools.Cryptography.Checksum
{

    internal static class HashCode64
    {

        public static long GetInt64HashCode(this string strText)
        {
            long hashCode = 0;

            if (!string.IsNullOrEmpty(strText))
            {
                byte[] hashText = Hashing.SHA256.ComputeSHA256(Encoding.Unicode.GetBytes(strText));
                hashCode = BitConverter.ToInt64(hashText, 0) ^ BitConverter.ToInt64(hashText, 8) ^ BitConverter.ToInt64(hashText, 24);
            }

            return (hashCode);
        }

    }

}
