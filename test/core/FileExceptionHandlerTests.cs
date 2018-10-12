using geheb.smart_backup.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace geheb.smart_backup.test.core
{
    public class FileExceptionHandlerTests : IDisposable
    {
        private readonly FileExceptionHandler _fileExceptionHandler = new FileExceptionHandler();
        private readonly string _tempFile;

        public FileExceptionHandlerTests()
        {
            _tempFile = Path.GetTempFileName();
        }

        [Fact]
        public void CanSkip_SharingViolation()
        {
            using (File.OpenWrite(_tempFile))
            {
                try
                {
                    using (File.OpenWrite(_tempFile))
                    {
                        Assert.False(true);
                    }
                }
                catch (Exception ex)
                {
                    Assert.True(_fileExceptionHandler.CanSkip(ex));
                }
            }
        }

        [Fact]
        public void CanSkip_NonExistentFile()
        {
            try
            {
                using (File.OpenRead("foo"))
                {
                    Assert.False(true);
                }
            }
            catch (Exception ex)
            {
                Assert.True(_fileExceptionHandler.CanSkip(ex));
            }
        }

        [Fact]
        public void CanSkip_UnauthorizedFile()
        {
            var tempFileInfo = new FileInfo(_tempFile)
            {
                IsReadOnly = true
            };

            try
            {
                tempFileInfo.Delete();
                Assert.False(true);
            }
            catch (Exception ex)
            {
                Assert.True(_fileExceptionHandler.CanSkip(ex));
            }
            finally
            {
                tempFileInfo.IsReadOnly = false;
            }
        }

        [Fact]
        public void CanSkip_CanceledException_ExpectsFalse()
        {
            Assert.False(_fileExceptionHandler.CanSkip(new OperationCanceledException()));
        }

        public void Dispose()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
        }
    }
}
