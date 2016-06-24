using System;
using System.Security.Cryptography;
using System.Text;

namespace nettools.Cryptography.Hashing
{

    internal static class SHA512
    {

        public static string ComputeSHA512(this string input)
        {
            using (HashAlgorithm algo = HashAlgorithm.Create("SHA512"))
                return HashToString(algo.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        public static byte[] ComputeSHA512(this byte[] input)
        {
            using (HashAlgorithm algo = HashAlgorithm.Create("SHA512"))
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
