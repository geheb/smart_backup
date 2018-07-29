using geheb.smart_backup.cli;
using geheb.smart_backup.crypto;
using geheb.smart_backup.io;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace geheb.smart_backup.core
{
    sealed class SigFileCreator : IDisposable
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static readonly string FileExtension = ".sig";

        readonly HashGenerator _hashGenerator = new HashGenerator();
        readonly string _currentBackupDirectory;
        readonly CancellationToken _cancel;

        public SigFileCreator(string currentBackupDirectory, CancellationToken cancel)
        {
            _currentBackupDirectory = currentBackupDirectory;
            _cancel = cancel;
        }

        public void Dispose()
        {
            _hashGenerator.Dispose();
        }

        public (string filePath, long filesChecked) CreateFull(BackupArgs backupArgs)
        {
            var filePath = Path.Combine(_currentBackupDirectory, "backup" + FileExtension);
            var filesChecked = 0L;

            _logger.Trace($"Create backup file: {filePath}");

            using (var fileEnumerator = new FileEnumerator(backupArgs.File, backupArgs.IgnoreRegexPattern, _cancel))
            {
                IEnumerable<FileInfo> items;
                var options = new ParallelOptions { CancellationToken = _cancel };
                var sha256Set = new Dictionary<FileInfo, string>();

                while ((items = fileEnumerator.Take(Environment.ProcessorCount)).Any())
                {
                    sha256Set.Clear();

                    Parallel.ForEach(items, options, fi => sha256Set.Add(fi, ComputeSha256(fi)));

                    foreach (var fileInfoAndSha256 in sha256Set)
                    {
                        filesChecked++;
                        File.AppendAllText(filePath,
                            new SigFileInfo(fileInfoAndSha256.Key, fileInfoAndSha256.Value).Serialize());
                    }
                }
            }

            if (filesChecked < 1)
            {
                _logger.Info("No files to backup found");
                File.WriteAllText(filePath, string.Empty);
            }

            return (filePath, filesChecked);
        }

        public async Task<FileInfo> Create(SigFileInfo metaInfo)
        {
            var sigFileInfo = new FileInfo(Path.Combine(_currentBackupDirectory, metaInfo.Sha256 + FileExtension));
            var sigFileTempInfo = new FileInfo(sigFileInfo.FullName + ".temp");

            try
            {
                if (sigFileInfo.Exists)
                {
                    await sigFileInfo.Copy(sigFileTempInfo.FullName, _cancel).ConfigureAwait(false);
                }

                using (var writer = new StreamWriter(FileFactory.Append(sigFileTempInfo.FullName)))
                {
                    await writer.WriteAsync(metaInfo.Serialize()).ConfigureAwait(false);
                }

                sigFileInfo.DeleteIfExists();
                File.Move(sigFileTempInfo.FullName, sigFileInfo.FullName);
            }
            finally
            {
                sigFileTempInfo.DeleteIfExists();
            }

            return sigFileInfo;
        }

        string ComputeSha256(FileInfo fileInfo)
        {
            _cancel.ThrowIfCancellationRequested();

            _logger.Trace($"Compute SHA256: {fileInfo.FullName}");
            var hash = _hashGenerator.Compute(fileInfo.FullName);

            _logger.Trace($"{fileInfo.FullName} = {hash}");

            return hash;
        }
    }
}
