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
    public class SigFileInfoTests : IDisposable
    {
        readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        readonly string _backupFile;

        public SigFileInfoTests()
        {
            Directory.CreateDirectory(_tempDirectory);
            _backupFile = Path.Combine(_tempDirectory, "foo");
            File.WriteAllText(_backupFile, "bar");
        }

        public void Dispose()
        {
            Directory.Delete(_tempDirectory, true);
        }

        [Fact]
        public void FromFileInfo_ExpectsValidProperties()
        {
            var fi = new FileInfo(_backupFile);
            var sigFile = SigFileInfo.FromFileInfo(fi, "0123456789012345678901234567890123456789012345678901234567890123");

            Assert.Equal(_backupFile, sigFile.Path);
            Assert.Equal(3, sigFile.Length);
            Assert.Equal(DateTime.UtcNow.AddMinutes(-1).Date, sigFile.CreationTimeUtc.Date);
            Assert.Equal("0123456789012345678901234567890123456789012345678901234567890123", sigFile.Sha256);
        }

        [Theory]
        [InlineData("foo\t1\t2000-01-01T01:02:03.0000000Z\t0123456789012345678901234567890123456789012345678901234567890123")]
        [InlineData("foo\t1\t2000-01-01T01:02:03Z\t0123456789012345678901234567890123456789012345678901234567890123")]
        [InlineData("foo\t1\t2000-01-01T01:02:03\t0123456789012345678901234567890123456789012345678901234567890123")]
        public void Parse_Line_ExpectsValidProperties(string line)
        {
            var sigFile = SigFileInfo.Parse(line);
            Assert.Equal("foo", sigFile.Path);
            Assert.Equal(1, sigFile.Length);
            Assert.Equal(new DateTime(2000,1,1,1,2,3), sigFile.CreationTimeUtc);
            Assert.Equal("0123456789012345678901234567890123456789012345678901234567890123", sigFile.Sha256);
        }

        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData("\t\t")]
        [InlineData("\t\t\t")]
        [InlineData("foo\tbar\tbaz\t1")]
        public void Parse_Invalid_ExpectsException(string line)
        {
            Assert.Throws<InvalidDataException>(() => SigFileInfo.Parse(line));
        }
    }
}
