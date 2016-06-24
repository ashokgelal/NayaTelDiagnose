using System;
using System.Security.Cryptography;
using System.Text;

namespace nettools.Cryptography.Hashing
{

    [Obsolete]
    internal static class MD5
    {

        [Obsolete]
        public static string ComputeMD5(this string input)
        {
            using (HashAlgorithm algo = HashAlgorithm.Create("MD5"))
                return HashToString(algo.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        [Obsolete]
        public static byte[] ComputeMD5(this byte[] input)
        {
            using (HashAlgorithm algo = HashAlgorithm.Create("MD5"))
                return algo.ComputeHash(input);
        }

        private static string HashToString(byte[] hashBytes)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));

            return sb.ToString();
        }

    }

}
