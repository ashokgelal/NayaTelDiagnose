using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace nettools.Cryptography.Checksum
{

    internal static class Crc32
    {

        public static string ComputeCrc32(this string input)
        {
            using (HashAlgorithm algo = new Crc32Algorithm())
                return HashToString(algo.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        public static byte[] ComputeCrc32(this byte[] input)
        {
            using (HashAlgorithm algo = new Crc32Algorithm())
                return algo.ComputeHash(input);
        }

        private static string HashToString(byte[] hashBytes)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));

            return sb.ToString();
        }

        private sealed class Crc32Algorithm : HashAlgorithm
        {
            public const uint DefaultPolynomial = 0xedb88320u;
            public const uint DefaultSeed = 0xffffffffu;

            static uint[] defaultTable;

            readonly uint seed;
            readonly uint[] table;
            uint hash;

            public Crc32Algorithm() : this(DefaultPolynomial, DefaultSeed)
            {
            }

            public Crc32Algorithm(uint polynomial, uint seed)
            {
                table = InitializeTable(polynomial);
                this.seed = hash = seed;
            }

            public override void Initialize()
            {
                hash = seed;
            }

            protected override void HashCore(byte[] array, int ibStart, int cbSize)
            {
                hash = CalculateHash(table, hash, array, ibStart, cbSize);
            }

            protected override byte[] HashFinal()
            {
                var hashBuffer = UInt32ToBigEndianBytes(~hash);
                HashValue = hashBuffer;
                return hashBuffer;
            }

            public override int HashSize
            {
                get
                {
                    return 32;
                }
            }

            public static uint Compute(byte[] buffer)
            {
                return Compute(DefaultSeed, buffer);
            }

            public static uint Compute(uint seed, byte[] buffer)
            {
                return Compute(DefaultPolynomial, seed, buffer);
            }

            public static uint Compute(uint polynomial, uint seed, byte[] buffer)
            {
                return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
            }

            static uint[] InitializeTable(uint polynomial)
            {
                if (polynomial == DefaultPolynomial && defaultTable != null)
                    return defaultTable;

                var createTable = new uint[256];
                for (var i = 0; i < 256; i++)
                {
                    var entry = (uint)i;
                    for (var j = 0; j < 8; j++)
                        if ((entry & 1) == 1)
                            entry = (entry >> 1) ^ polynomial;
                        else
                            entry = entry >> 1;
                    createTable[i] = entry;
                }

                if (polynomial == DefaultPolynomial)
                    defaultTable = createTable;

                return createTable;
            }

            static uint CalculateHash(uint[] table, uint seed, IList<byte> buffer, int start, int size)
            {
                var crc = seed;
                for (var i = start; i < size - start; i++)
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                return crc;
            }

            static byte[] UInt32ToBigEndianBytes(uint num)
            {
                var result = BitConverter.GetBytes(num);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(result);

                return result;
            }
        }

    }

}
