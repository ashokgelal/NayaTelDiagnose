using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace nettools.Cryptography.Checksum
{

    internal static class Crc64
    {

        public static string ComputeCrc64(this string input)
        {
            using (HashAlgorithm algo = new Crc64Iso())
                return HashToString(algo.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        public static byte[] ComputeCrc64(this byte[] input)
        {
            using (HashAlgorithm algo = new Crc64Iso())
                return algo.ComputeHash(input);
        }

        private static string HashToString(byte[] hashBytes)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));

            return sb.ToString();
        }

        private class Crc64Algorithm : HashAlgorithm
        {

            public const ulong DefaultSeed = 0x0;

            readonly ulong[] table;
            readonly ulong seed;

            ulong hash;

            public Crc64Algorithm(ulong polynomial) : this(polynomial, DefaultSeed)
            {
            }

            public Crc64Algorithm(ulong polynomial, ulong seed)
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
                hash = CalculateHash(hash, table, array, ibStart, cbSize);
            }

            protected override byte[] HashFinal()
            {
                var hashBuffer = ulongToBigEndianBytes(hash);
                HashValue = hashBuffer;
                return hashBuffer;
            }

            public override int HashSize
            {
                get
                {
                    return 64;
                }
            }

            protected static ulong CalculateHash(ulong seed, ulong[] table, IList<byte> buffer, int start, int size)
            {
                var crc = seed;

                for (var i = start; i < size; i++)
                    unchecked
                    {
                        crc = (crc >> 8) ^ table[(buffer[i] ^ crc) & 0xff];
                    }

                return crc;
            }

            static byte[] ulongToBigEndianBytes(ulong value)
            {
                var result = BitConverter.GetBytes(value);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(result);

                return result;
            }

            static ulong[] InitializeTable(ulong polynomial)
            {
                if (polynomial == Crc64Iso.Iso3309Polynomial && Crc64Iso.Table != null)
                    return Crc64Iso.Table;

                var createTable = CreateTable(polynomial);

                if (polynomial == Crc64Iso.Iso3309Polynomial)
                    Crc64Iso.Table = createTable;

                return createTable;
            }

            protected static ulong[] CreateTable(ulong polynomial)
            {
                var createTable = new ulong[256];
                for (var i = 0; i < 256; ++i)
                {
                    var entry = (ulong)i;
                    for (var j = 0; j < 8; ++j)
                        if ((entry & 1) == 1)
                            entry = (entry >> 1) ^ polynomial;
                        else
                            entry = entry >> 1;
                    createTable[i] = entry;
                }
                return createTable;
            }
        }

        private class Crc64Iso : Crc64Algorithm
        {
            internal static ulong[] Table;

            public const ulong Iso3309Polynomial = 0xD800000000000000;

            public Crc64Iso() : base(Iso3309Polynomial)
            {
            }

            public Crc64Iso(ulong seed) : base(Iso3309Polynomial, seed)
            {
            }

            public static ulong Compute(byte[] buffer)
            {
                return Compute(DefaultSeed, buffer);
            }

            public static ulong Compute(ulong seed, byte[] buffer)
            {
                if (Table == null)
                    Table = CreateTable(Iso3309Polynomial);

                return CalculateHash(seed, Table, buffer, 0, buffer.Length);
            }
        }

    }

}
