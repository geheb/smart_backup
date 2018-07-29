using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace geheb.smart_backup.io
{
    internal static class FileExtensions
    {
        public static async Task Copy(this FileInfo sourceFile, string destinationFile, CancellationToken cancel)
        {
            try
            {
                using (var sourceStream = FileFactory.Open(sourceFile.FullName))
                using (var destinationStream = FileFactory.Create(destinationFile))
                {
                    await sourceStream.CopyToAsync(destinationStream, 1024 * 1024, cancel).ConfigureAwait(false);
                }
            }
            catch
            {
                new FileInfo(destinationFile).DeleteIfExists();
                throw;
            }
        }

        public static bool DeleteIfExists(this FileInfo sourceFile)
        {
            if (!sourceFile.Exists) return false;
            sourceFile.Delete();
            return true;
        }
    }
}
