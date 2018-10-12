using System;
using System.IO;
using System.Security.Cryptography;

namespace geheb.smart_backup.crypto
{
    internal sealed class HashGenerator : IDisposable
    {
        private const string EmptyFileHash = "0000000000000000000000000000000000000000000000000000000000000000";

        private readonly HashAlgorithm _hash = SHA256.Create();

        public void Dispose()
        {
            _hash.Dispose();
        }

        public string Compute(string file)
        {
            var fi = new FileInfo(file);
            if (fi.Length < 1)
                return EmptyFileHash;

            using (var bf = new BufferedStream(fi.OpenRead(), 1024 * 1024))
            {
                var hash = BitConverter.ToString(_hash.ComputeHash(bf)).Replace("-", string.Empty).ToLowerInvariant();
                return hash;
            }
        }
    }
}
