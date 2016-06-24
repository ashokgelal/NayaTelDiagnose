using System;
using System.Security.Cryptography;
using System.Text;

namespace nettools.Cryptography.Hashing
{

    internal static class SHA256
    {

        public static string ComputeSHA256(this string input)
        {
            using (HashAlgorithm algo = HashAlgorithm.Create("SHA256"))
                return HashToString(algo.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        public static byte[] ComputeSHA256(this byte[] input)
        {
            using (HashAlgorithm algo = HashAlgorithm.Create("SHA256"))
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
