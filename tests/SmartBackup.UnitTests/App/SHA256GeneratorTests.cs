using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Geheb.SmartBackup.App;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Geheb.SmartBackup.UnitTests.App
{
    [TestClass]
    public class SHA256GeneratorTests
    {
        private readonly IFileSystem _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "empty.txt", new MockFileData(string.Empty) },
            { "foobar.txt", new MockFileData("foobar") }
        });

        private readonly SHA256Generator _generator = new SHA256Generator();

        [TestMethod]
        public void GenerateFrom_EmptyFile_ExpectsZeroValues()
        {
            var file = _fileSystem.FileInfo.FromFileName("empty.txt");
            Assert.AreEqual("0000000000000000000000000000000000000000000000000000000000000000", _generator.GenerateFrom(file));
        }

        [TestMethod]
        public void GenerateFrom_FileWithContent_ExpectsValue()
        {
            var file = _fileSystem.FileInfo.FromFileName("foobar.txt");
            Assert.AreEqual("c3ab8ff13720e8ad9047dd39466b3c8974e592c2fa383d4a3960714caef0c4f2", _generator.GenerateFrom(file));
        }

        [TestMethod]
        public void GenerateFrom_FileNonExistent_ThrowsException()
        {
            var invalidFile = _fileSystem.FileInfo.FromFileName("invalid.txt");
            Assert.ThrowsException<FileNotFoundException>(() => _generator.GenerateFrom(invalidFile));
        }
    }
}
