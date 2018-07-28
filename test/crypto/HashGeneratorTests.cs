using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using geheb.smart_backup.crypto;
using Xunit;

namespace geheb.smart_backup.test.crypto
{
    public class HashGeneratorTests : IDisposable
    {
        readonly HashGenerator _hashGenerator = new HashGenerator();

        public void Dispose()
        {
            _hashGenerator.Dispose();
        }

        [Fact]
        public void ComputeHash_EmptyFile_ExpectsZeroValues()
        {
            var emptyFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                File.WriteAllText(emptyFile, string.Empty);
                Assert.Equal("0000000000000000000000000000000000000000000000000000000000000000", _hashGenerator.Compute(emptyFile));
            }
            finally
            {
                File.Delete(emptyFile);
            }
        }

        [Fact]
        public void ComputeHash_FileWithContent_ExpectsValue()
        {
            var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                File.WriteAllText(file, "foobar");
                Assert.Equal("c3ab8ff13720e8ad9047dd39466b3c8974e592c2fa383d4a3960714caef0c4f2", _hashGenerator.Compute(file));
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Fact]
        public void ComputeHash_FileNonExistent_ThrowsException()
        {
            var emptyFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Assert.Throws<FileNotFoundException>(() => _hashGenerator.Compute(emptyFile));
        }
    }
}
