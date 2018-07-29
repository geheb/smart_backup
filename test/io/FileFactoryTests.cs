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
    public class FileFactoryTests : IDisposable
    {
        readonly string _file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public FileFactoryTests()
        {
            File.WriteAllLines(_file, new[] { "foo", "bar" });
        }

        public void Dispose()
        {
            File.Delete(_file);
        }

        [Fact]
        public async Task Append_ExistentFile_ExpectsContent()
        {
            using (var stream = new StreamWriter(FileFactory.Append(_file)))
            {
                await stream.WriteLineAsync("baz");
            }
            var lines = File.ReadAllLines(_file);

            Assert.Collection(lines,
                item => Assert.Contains("foo", item),
                item => Assert.Contains("bar", item),
                item => Assert.Contains("baz", item));
        }

        [Fact]
        public async Task Open_ExistentFile_ExpectsContent()
        {
            var lines = new List<string>();
            using (var stream = new StreamReader(FileFactory.Open(_file)))
            {
                string line;
                while ((line = await stream.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }

            Assert.Collection(lines,
                item => Assert.Contains("foo", item),
                item => Assert.Contains("bar", item));
        }

        [Fact]
        public void Create_File_ExpectsEmpty()
        {
            using (FileFactory.Create(_file))
            {
            }
            var content = File.ReadAllText(_file);
            Assert.Empty(content);
        }
    }
}
