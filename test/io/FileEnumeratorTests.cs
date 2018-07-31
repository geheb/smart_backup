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
    public class FileEnumeratorTests : IDisposable
    {
        readonly string _tempDirectory1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        readonly string _tempDirectory2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        readonly string _tempFile;

        public FileEnumeratorTests()
        {
            Directory.CreateDirectory(_tempDirectory1);
            Directory.CreateDirectory(_tempDirectory2);

            _tempFile = Path.Combine(_tempDirectory1, "foo");
            File.WriteAllText(_tempFile, string.Empty);
            File.WriteAllText(Path.Combine(_tempDirectory2, "bar"), string.Empty);
            File.WriteAllText(Path.Combine(_tempDirectory2, "baz"), string.Empty);
        }

        public void Dispose()
        {
            Directory.Delete(_tempDirectory1, true);
            Directory.Delete(_tempDirectory2, true);
        }

        [Fact]
        public void Take_MoreThenAvailable_ExpectsCount()
        {
            using (var enumerator = new FileEnumerator(new[] { _tempFile, _tempDirectory2 }, 
                null, CancellationToken.None))
            {
                var items = enumerator.Take(4);
                Assert.Equal(3, items.Count());
            }
        }

        [Fact]
        public void Take_All_ExpectsOrder()
        {
            using (var enumerator = new FileEnumerator(new[] { _tempFile, _tempDirectory2 },
                null, CancellationToken.None))
            {
                var items = enumerator.Take(3);

                Assert.Collection(items,
                    item => Assert.Contains("foo", item.Name),
                    item => Assert.Contains("bar", item.Name),
                    item => Assert.Contains("baz", item.Name));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Take_InvalidCount_ExpectsEmpty(int count)
        {
            using (var enumerator = new FileEnumerator(new[] { _tempFile, _tempDirectory2 },
                null, CancellationToken.None))
            {
                Assert.Empty(enumerator.Take(count));
            }
        }

        [Fact]
        public void Take_Last_ExpectsEmpty()
        {
            using (var enumerator = new FileEnumerator(new[] { _tempFile, _tempDirectory2 },
                null, CancellationToken.None))
            {
                enumerator.Take(3);

                Assert.Empty(enumerator.Take(1));
            }
        }

        [Fact]
        public void Take_NonExistentDirectory_ExpectsEmpty()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            using (var enumerator = new FileEnumerator(new[] { directory },
                null, CancellationToken.None))
            {
                Assert.Empty(enumerator.Take(1));
            }
        }

        [Fact]
        public void Take_WithIngorePattern_ExpectsFiltered()
        {
            using (var enumerator = new FileEnumerator(new[] { _tempFile, _tempDirectory2 },
                new[] { "BA[rz]" }, CancellationToken.None))
            {
                var items = enumerator.Take(3);

                var item = Assert.Single(items);
                Assert.Equal("foo", item.Name);
            }
        }

        [Fact(Skip = "can be run without admin privileges only")]
        public void Take_UnauthorizedAccess_ExpectsEmpty()
        {
            var windowsTempDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");

            using (var enumerator = new FileEnumerator(new[] { windowsTempDirectory },
                null, CancellationToken.None))
            {
                var items = enumerator.Take(1);

                Assert.Empty(items);
            }
        }
    }
}
