using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace geheb.smart_backup.io
{
    static class FileExtensions
    {
        public static async Task Copy(this FileInfo sourceFile, string destinationFile, CancellationToken cancel)
        {
            try
            {
                using (var sourceStream = FileFactory.OpenAsFileStream(sourceFile.FullName))
                using (var destinationStream = FileFactory.CreateAsFileStream(destinationFile))
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

        public static void DeleteIfExists(this FileInfo sourceFile)
        {
            if (sourceFile.Exists)
            {
                sourceFile.Delete();
            }
        }
    }
}
