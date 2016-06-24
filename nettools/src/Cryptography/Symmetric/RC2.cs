using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace nettools.Cryptography.Symmetric
{

    [Obsolete]
    internal static class RC2
    {

        [Obsolete]
        public static string EncryptRC2(this string input, string password, int iterations = 500000)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(input);
            
            using (Rfc2898DeriveBytes passBytes = new Rfc2898DeriveBytes(password, 16, iterations))
            {
                byte[] keyBytes = passBytes.GetBytes(32);

                using (SymmetricAlgorithm algo = SymmetricAlgorithm.Create("RC2"))
                {
                    algo.Mode = CipherMode.CBC;
                    algo.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = algo.CreateEncryptor(keyBytes, passBytes.Salt))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                return Convert.ToBase64String(memoryStream.ToArray());
                            }
                        }
                    }
                }
            }
        }

        [Obsolete]
        public static string DecryptRC2(this string input, string password, int iterations = 500000)
        {
            byte[] cipherTextBytes = Encoding.UTF8.GetBytes(input);

            using (Rfc2898DeriveBytes passBytes = new Rfc2898DeriveBytes(password, 16, iterations))
            {
                byte[] keyBytes = passBytes.GetBytes(32);

                using (SymmetricAlgorithm algo = SymmetricAlgorithm.Create("RC2"))
                {
                    algo.Mode = CipherMode.CBC;
                    algo.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = algo.CreateDecryptor(keyBytes, passBytes.Salt))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

    }

}
