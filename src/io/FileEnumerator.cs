using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace geheb.smart_backup.io
{
    internal sealed class FileEnumerator : IDisposable
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly List<FileInfo> _currentFiles = new List<FileInfo>();
        private readonly IEnumerator<FileInfo> _currentFileEnumerator;
        private readonly List<Regex> _ignorePatterns = new List<Regex>();

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

            while (count > 0 && _currentFiles.Count < count && _currentFileEnumerator.MoveNext())
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

        private IEnumerator<FileInfo> Enumerate(IEnumerable<string> path, CancellationToken cancel)
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

                    foreach (var fileInfo in EnumerateAllFiles(directoryInfo))
                    {
                        cancel.ThrowIfCancellationRequested();

                        if (HasIgnorePatternMatched(fileInfo)) continue;

                        yield return fileInfo;
                    }
                }
            }
        }

        private IEnumerable<FileInfo> EnumerateAllFiles(DirectoryInfo directoryInfo)
        {
            IEnumerable<FileInfo> files = null;
            try
            {
                files = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Warn(ex);
                yield break;
            }

            using (var enumerator = files.GetEnumerator())
            {
                bool? hasCurrent;
                do
                {
                    hasCurrent = null;
                    try
                    {
                        hasCurrent = enumerator.MoveNext();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        _logger.Warn(ex);
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
