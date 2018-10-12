using geheb.smart_backup.cli;
using geheb.smart_backup.crypto;
using geheb.smart_backup.io;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace geheb.smart_backup.core
{
    sealed class SigFileCreator : IDisposable
    {
        public static readonly string FileExtension = ".sig";
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly HashGenerator _hashGenerator = new HashGenerator();
        private readonly FileExceptionHandler _fileExceptionHandler = new FileExceptionHandler();
        private readonly IShutdownHandler _shutdownHandler;

        public SigFileCreator(IShutdownHandler shutdownHandler)
        {
            _shutdownHandler = shutdownHandler;
        }

        public void Dispose()
        {
            _hashGenerator.Dispose();
        }

        public SigFileResult CreateFull(BackupArgs backupArgs, string currentBackupDirectory)
        {
            var filePath = Path.Combine(currentBackupDirectory, "backup" + FileExtension);
            var filesChecked = 0L;

            _logger.Trace($"Create backup file: {filePath}");

            using (var fileEnumerator = new FileEnumerator(backupArgs.File, backupArgs.IgnoreRegexPattern, _shutdownHandler.Token))
            {
                IEnumerable<FileInfo> items;
                var options = new ParallelOptions { CancellationToken = _shutdownHandler.Token };
                var sha256Set = new Dictionary<FileInfo, string>();

                while ((items = fileEnumerator.Take(Environment.ProcessorCount)).Any())
                {
                    sha256Set.Clear();

                    Parallel.ForEach(items, options, fi => sha256Set.Add(fi, ComputeSha256(fi)));
                    foreach (var fileInfoAndSha256 in sha256Set)
                    {
                        if (string.IsNullOrEmpty(fileInfoAndSha256.Value)) // skip files with access error
                        {
                            continue;
                        }

                        filesChecked++;
                        File.AppendAllText(filePath,
                            SigFileInfo.FromFileInfo(fileInfoAndSha256.Key, fileInfoAndSha256.Value).Serialize());
                    }
                }
            }

            if (filesChecked < 1)
            {
                _logger.Info("No files to backup found");
                File.WriteAllText(filePath, string.Empty);
            }

            return new SigFileResult(filePath, filesChecked);
        }

        public async Task<FileInfo> Create(SigFileInfo metaInfo, string currentBackupDirectory)
        {
            var sigFileInfo = new FileInfo(Path.Combine(currentBackupDirectory, metaInfo.Sha256 + FileExtension));
            var sigFileTempInfo = new FileInfo(sigFileInfo.FullName + ".temp");

            try
            {
                if (sigFileInfo.Exists)
                {
                    await sigFileInfo.Copy(sigFileTempInfo.FullName, _shutdownHandler.Token).ConfigureAwait(false);
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

        private string ComputeSha256(FileInfo fileInfo)
        {
            _shutdownHandler.Token.ThrowIfCancellationRequested();

            try
            {
                _logger.Trace($"Compute SHA256: {fileInfo.FullName}");
                var hash = _hashGenerator.Compute(fileInfo.FullName);

                _logger.Trace($"{fileInfo.FullName} = {hash}");

                return hash;
            }
            catch (Exception ex)
            {
                if (!_fileExceptionHandler.CanSkip(ex)) throw;
                _logger.Warn($"File access failed: {ex.Message}");
                return null;
            }
        }
    }
}
