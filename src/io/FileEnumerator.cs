using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace geheb.smart_backup.io
{
    sealed class FileEnumerator : IDisposable
    {
        readonly List<FileInfo> _currentFiles = new List<FileInfo>();
        readonly IEnumerator<FileInfo> _currentFileEnumerator;
        readonly List<Regex> _ignorePatterns = new List<Regex>();

        public FileEnumerator(IEnumerable<string> path, IEnumerable<string> ignorePattern, CancellationToken cancel)
        {
            if (ignorePattern != null && ignorePattern.Any())
            {
                _ignorePatterns.AddRange(ignorePattern.Select(p => new Regex(p, RegexOptions.Compiled | RegexOptions.IgnoreCase)));
            }
            _currentFileEnumerator = Enumerate(path, cancel);
        }

        public void Dispose()
        {
            _currentFileEnumerator.Dispose();
        }

        public IEnumerable<FileInfo> Take(int count)
        {
            _currentFiles.Clear();

            while (_currentFiles.Count < count && _currentFileEnumerator.MoveNext())
            {
                _currentFiles.Add(_currentFileEnumerator.Current);
            }

            return _currentFiles;
        }

        bool HasIgnorePatternMatched(FileInfo fi)
        {
            if (_ignorePatterns.Count < 1) return false;

            return _ignorePatterns.Any(regex => regex.IsMatch(fi.FullName));
        }

        IEnumerator<FileInfo> Enumerate(IEnumerable<string> path, CancellationToken cancel)
        {
            foreach (string p in path)
            {
                cancel.ThrowIfCancellationRequested();

                var pathInfo = new FileInfo(p);

                if (pathInfo.Exists) // it is a file
                {
                    if (HasIgnorePatternMatched(pathInfo)) continue;

                    yield return pathInfo;
                }
                else
                {
                    var directoryInfo = new DirectoryInfo(p);
                    if (!directoryInfo.Exists) continue;

                    foreach (var fileInfo in
                        EnumerateAndCatchUnauthorizedAccess(directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)))
                    {
                        cancel.ThrowIfCancellationRequested();

                        if (HasIgnorePatternMatched(fileInfo)) continue;

                        yield return fileInfo;
                    }
                }
            }
        }

        IEnumerable<T> EnumerateAndCatchUnauthorizedAccess<T>(IEnumerable<T> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                bool? hasCurrent;
                do
                {
                    hasCurrent = null;
                    try
                    {
                        hasCurrent = enumerator.MoveNext();
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }

                    if (hasCurrent ?? false)
                    {
                        yield return enumerator.Current;
                    }

                } while (hasCurrent ?? true); 
            }
        }
    }
}
