using System;
using System.IO;
using System.Security.Cryptography;

namespace Geheb.SmartBackup.App
{
    class SHA256Generator
    {
        private const string EmptyFileHash = "0000000000000000000000000000000000000000000000000000000000000000";

        public string GenerateFrom(FileInfo fileInfo)
        {
            if (fileInfo.Length < 1)
                return EmptyFileHash;

            using var sha256 = SHA256.Create();

            using var bf = new BufferedStream(fileInfo.OpenRead(), 1024 * 1024 * 10);

            var hash = BitConverter.ToString(sha256.ComputeHash(bf)).Replace("-", string.Empty).ToLowerInvariant();
            return hash;
        }
    }
}
