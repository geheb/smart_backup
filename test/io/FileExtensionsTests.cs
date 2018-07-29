using geheb.smart_backup.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace geheb.smart_backup.test.io
{
    public class FileExtensionsTests
    {
        [Fact]
        public async Task Copy_File_ExpectsContent()
        {
            var sourceFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var targetFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                File.WriteAllText(sourceFile, "foo");

                var fileInfo = new FileInfo(sourceFile);
                await fileInfo.Copy(targetFile, CancellationToken.None);

                var content = File.ReadAllText(targetFile);
                Assert.Equal("foo", content);
            }
            finally
            {
                File.Delete(sourceFile);
                File.Delete(targetFile);
            }
        }

        [Fact]
        public void Delete_File_ExpectsDeleted()
        {
            var sourceFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                File.WriteAllText(sourceFile, "foo");

                var fileInfo = new FileInfo(sourceFile);
                Assert.True(fileInfo.DeleteIfExists());
            }
            finally
            {
                File.Delete(sourceFile);
            }
        }

        [Fact]
        public void Delete_NonExistentFile_ExpectsNotDeleted()
        {
            var sourceFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                var fileInfo = new FileInfo(sourceFile);
                Assert.False(fileInfo.DeleteIfExists());
            }
            finally
            {
                File.Delete(sourceFile);
            }
        }
    }
}
