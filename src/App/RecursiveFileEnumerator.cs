using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Geheb.SmartBackup.App
{
    class RecursiveFileEnumerator
    {
        private readonly IFileSystem _fileSystem;

        public RecursiveFileEnumerator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string[]> Enumerate(string path, int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));

            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
                ReturnSpecialDirectories = false,
                RecurseSubdirectories = true
            };

            var files = new List<string>();

            foreach (var file in _fileSystem.Directory.EnumerateFiles(path, "*", options))
            {
                files.Add(file);
                if (files.Count == count)
                {
                    yield return files.ToArray();
                    files.Clear();
                }
            }

            if (files.Count > 0)
            {
                yield return files.ToArray();
            }
        }
    }
}
