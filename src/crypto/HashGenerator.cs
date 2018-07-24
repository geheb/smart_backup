using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace geheb.smart_backup.crypto
{
    sealed class HashGenerator : IDisposable
    {
        readonly HashAlgorithm _hash = SHA256.Create();

        public void Dispose()
        {
            _hash.Dispose();
        }

        public string Compute(string file)
        {
            var fi = new FileInfo(file);
            if (fi.Length < 1)
                return "0000000000000000000000000000000000000000000000000000000000000000";

            using (var bf = new BufferedStream(fi.OpenRead(), 1024 * 1024))
            {
                var hash = BitConverter.ToString(_hash.ComputeHash(bf)).Replace("-", string.Empty).ToLowerInvariant();
                return hash;
            }
        }
    }
}
