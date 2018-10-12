using geheb.smart_backup.core;
using Moq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;

namespace geheb.smart_backup.test.core
{
    public class CompressCliTests : IDisposable
    {
        readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        readonly AppSettings _appSettings;

        public CompressCliTests()
        {
            Directory.CreateDirectory(_tempDirectory);

            _appSettings = AppSettings.Load(TextWriter.Null);
            _appSettings.CompressApp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7za.exe");
        }

        public void Dispose()
        {
            Directory.Delete(_tempDirectory, true);
        }

        [Fact]
        public void Validate_AppSettings()
        {
            Assert.True(_appSettings.Validate(TextWriter.Null));
        }

        [Fact]
        public void Compress_File()
        {
            var password = Guid.NewGuid().ToString("N");
            var mockShutdownHandler = new Mock<IShutdownHandler>();
            mockShutdownHandler.Setup(inst => inst.Token).Returns(CancellationToken.None);

            var compress = new CompressCli(_appSettings, mockShutdownHandler.Object);
            var sourceFile = new FileInfo(Path.Combine(_tempDirectory, Guid.NewGuid().ToString()));
            File.WriteAllText(sourceFile.FullName, "foo bar");
            var targetFile = new FileInfo(Path.Combine(_tempDirectory, Guid.NewGuid().ToString()));

            var hasCompressed = compress.Compress(sourceFile, targetFile, password);

            Assert.True(hasCompressed);
        }

        [Fact]
        public void Compress_File_ExpectsContent()
        {
            var password = Guid.NewGuid().ToString("N");
            var mockShutdownHandler = new Mock<IShutdownHandler>();
            mockShutdownHandler.Setup(inst => inst.Token).Returns(CancellationToken.None);

            var compress = new CompressCli(_appSettings, mockShutdownHandler.Object);
            var sourceFile = new FileInfo(Path.Combine(_tempDirectory, Guid.NewGuid().ToString()));
            var content = Guid.NewGuid().ToString();
            File.WriteAllText(sourceFile.FullName, content);
            var targetFile = new FileInfo(Path.Combine(_tempDirectory,
                Guid.NewGuid() + "." + _appSettings.CompressFileExtension));

            compress.Compress(sourceFile, targetFile, password);

            var uncompressedContent = File.ReadAllText(UncompressFile(targetFile.FullName, password));
            Assert.Equal(content, uncompressedContent);
        }

        string UncompressFile(string file, string password)
        {
            var outputDirectory = Path.Combine(_tempDirectory, Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDirectory);

            var procInfo = new ProcessStartInfo(_appSettings.CompressApp, $"e \"{file}\" -p\"{password}\" -o\"{outputDirectory}\"")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false,
                WorkingDirectory = _tempDirectory
            };

            using (var proc = Process.Start(procInfo))
            {
                proc.WaitForExit();
                if (proc.ExitCode != 0) throw new InvalidOperationException("uncompress failed");

                return Directory.GetFiles(outputDirectory)[0];
            }
        }
    }
}
