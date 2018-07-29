using geheb.smart_backup.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace geheb.smart_backup.test.io
{
    public class BatchFileReaderTests : IDisposable
    {
        readonly string _file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public BatchFileReaderTests()
        {
            File.WriteAllLines(_file, new[] { "foo", "bar", "baz" });
        }

        public void Dispose()
        {
            File.Delete(_file);
        }

        [Fact]
        public async Task Take_MoreThenAvailable_ExpectsCount()
        {
            using (var reader = new BatchFileReader(_file))
            {
                var items = await reader.Take(4);

                Assert.Equal(3, items.Count());
            }
        }

        [Fact]
        public async Task Take_All_ExpectsOrder()
        {
            using (var reader = new BatchFileReader(_file))
            {
                var items = await reader.Take(3);

                Assert.Collection(items,
                    item => Assert.Contains("foo", item),
                    item => Assert.Contains("bar", item),
                    item => Assert.Contains("baz", item));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Take_InvalidCount_ExpectsEmpty(int count)
        {
            using (var reader = new BatchFileReader(_file))
            {
                Assert.Empty(await reader.Take(count));
            }
        }

        [Fact]
        public async Task Take_Last_ExpectsEmpty()
        {
            using (var reader = new BatchFileReader(_file))
            {
                await reader.Take(3);

                Assert.Empty(await reader.Take(1));
            }
        }
    }
}
