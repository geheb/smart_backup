using System;
using System.Collections.Generic;
using System.IO;

namespace Geheb.SmartBackup.App
{
    class RecursiveFileEnumerator
    {
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

            foreach (var file in Directory.EnumerateFiles(path, "*", options))
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
