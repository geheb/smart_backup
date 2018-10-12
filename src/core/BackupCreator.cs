using geheb.smart_backup.cli;
using geheb.smart_backup.crypto;
using geheb.smart_backup.io;
using geheb.smart_backup.text;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace geheb.smart_backup.core
{
    sealed class BackupCreator : IDisposable
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly HashGenerator _hashGenerator = new HashGenerator();
        readonly FileExceptionHandler _fileExceptionHandler = new FileExceptionHandler();
        readonly BackupArgs _backupArgs;
        readonly AppSettings _appSettings;
        readonly CancellationToken _cancel;
        readonly CompressCli _compressCli;
        readonly IDictionary<string, byte> _currentBackupDirectoryStructure = 
            new SortedDictionary<string, byte>(new DescendedStringComparer(StringComparison.OrdinalIgnoreCase));
        string _currentBackupDirectory;
        long _filesChecked, _filesProcessed, _filesSkipped;
        string _fullSigFile;

        public BackupCreator(AppSettings appSettings, CancellationToken cancel, BackupArgs args)
        {
            _appSettings = appSettings;
            _cancel = cancel;
            _backupArgs = args;
            _compressCli = new CompressCli(appSettings, args.Password, cancel);
        }

        public void Dispose()
        {
            _hashGenerator.Dispose();
        }

        public async Task Create()
        {
            ReadCurrentBackupDirectoryStructure();

            if (!CreateBackupDirectoryAndSigFile()) return;

            await BackupFiles().ConfigureAwait(false);

            _logger.Info($"Files checked: {_filesChecked}, files processed: {_filesProcessed}, files skipped: {_filesSkipped}");

            CleanupBackupDirectory();
        }

        void ReadCurrentBackupDirectoryStructure()
        {
            if (_backupArgs.MaxBackupSets < 1) return;

            // read all directories and order by descending
            foreach (var dir in Directory.EnumerateDirectories(_backupArgs.Target, "*", SearchOption.TopDirectoryOnly))
            {
                var dirInfo = new DirectoryInfo(dir);
                var timeIndex = dirInfo.Name.IndexOf('T');
                if (timeIndex < 0) continue;
                var name = dirInfo.Name.Substring(0, timeIndex) + dirInfo.Name.Substring(timeIndex).Replace('-', ':');

                bool isValid = DateTime.TryParse(name, out _);
                bool hasSigFiles = dirInfo.EnumerateFiles("*" + SigFileCreator.FileExtension, SearchOption.TopDirectoryOnly).Any();

                if (isValid && hasSigFiles)
                {
                    _currentBackupDirectoryStructure.Add(dir, 0);
                }
            }

            // leave latest ones only
            int count = 1; // current backup directory included
            foreach (var directory in _currentBackupDirectoryStructure.Keys.ToArray())
            {
                if (++count > _backupArgs.MaxBackupSets)
                {
                    _currentBackupDirectoryStructure.Remove(directory);
                }
            }
        }

        IEnumerable<string> FindExistentSigFiles(SigFileInfo sigFile)
        {
            var files = new List<string>();
            var searchPattern = sigFile.Sha256 + SigFileCreator.FileExtension;

            var file = Directory.EnumerateFiles(_currentBackupDirectory, searchPattern, SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (file != null)
            {
                files.Add(file);
            }

            if (_backupArgs.MaxBackupSets < 1) return files;

            foreach (var directory in _currentBackupDirectoryStructure.Keys)
            {
                file = Directory.EnumerateFiles(directory, searchPattern, SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (file != null)
                {
                    files.Add(file);
                }
            }

            return files;
        }

        void CleanupBackupDirectory()
        {
            if (_backupArgs.MaxBackupSets < 1) return;

            _logger.Trace("Clean up backup directory...");

            foreach (var directory in Directory.EnumerateDirectories(_backupArgs.Target, "*", SearchOption.TopDirectoryOnly))
            {
                if (_currentBackupDirectory.Equals(directory, StringComparison.OrdinalIgnoreCase)) continue;

                if (!_currentBackupDirectoryStructure.ContainsKey(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        bool CreateBackupDirectoryAndSigFile()
        {
            _currentBackupDirectory = Path.Combine(_backupArgs.Target, DateTime.UtcNow.ToString("o").Replace(':', '-'));
            _logger.Trace($"Create backup directory: {_currentBackupDirectory}");
            Directory.CreateDirectory(_currentBackupDirectory);

            using (var sigFileCreator = new SigFileCreator(_currentBackupDirectory, _cancel))
            {
                (_fullSigFile, _filesChecked) = sigFileCreator.CreateFull(_backupArgs);
                return _filesChecked > 0;
            }
        }

        async Task BackupFiles()
        {
            using (var reader = new BatchFileReader(_fullSigFile))
            {
                IEnumerable<string> items;
                var duplicateSigFiles = new List<SigFileInfo>();
                var uniqueSigFiles = new Dictionary<string, SigFileInfo>();

                while ((items = await reader.Take(Environment.ProcessorCount).ConfigureAwait(false)).Any())
                {
                    uniqueSigFiles.Clear();
                    duplicateSigFiles.Clear();

                    foreach (var item in items)
                    {
                        var sigFile = SigFileInfo.Parse(item);
                        var sourceFileInfo = new FileInfo(sigFile.Path);
                        if (!sourceFileInfo.Exists)
                        {
                            _logger.Warn($"File has been deleted: {sourceFileInfo.FullName}");
                            Interlocked.Increment(ref _filesSkipped);
                            continue;
                        }

                        if (!UpdateHashIfModifed(sigFile, sourceFileInfo)) continue;

                        if (!uniqueSigFiles.ContainsKey(sigFile.Sha256))
                        {
                            uniqueSigFiles.Add(sigFile.Sha256, sigFile);
                        }
                        else
                        {
                            duplicateSigFiles.Add(sigFile);
                        }
                    }

                    if (uniqueSigFiles.Count > 0)
                    {
                        await Task.WhenAll(uniqueSigFiles.Values.Select(m => BackupFile(m))).ConfigureAwait(false);
                    }

                    foreach (var sigFile in duplicateSigFiles)
                    {
                        await BackupFile(sigFile).ConfigureAwait(false);
                    }
                }
            }
        }

        bool UpdateHashIfModifed(SigFileInfo sigFile, FileInfo sourceFileInfo)
        {
            try
            {
                if (!sigFile.IsFileModified) return true;

                var hash = ComputeSha256(sourceFileInfo);
                sigFile.Update(sourceFileInfo, hash);
                return true;
            }
            catch (Exception ex)
            {
                if (!_fileExceptionHandler.CanSkip(ex)) throw;

                _logger.Warn($"File access failed: {ex.Message}");
                Interlocked.Increment(ref _filesSkipped);
                return false;
            }
        }

        string ComputeSha256(FileInfo fileInfo)
        {
            _cancel.ThrowIfCancellationRequested();

            _logger.Trace($"Compute SHA256: {fileInfo.FullName}");
            var hash = _hashGenerator.Compute(fileInfo.FullName);

            _logger.Trace($"{fileInfo.FullName} = {hash}");

            return hash;
        }

        async Task BackupFile(SigFileInfo sigFile)
        {
            _cancel.ThrowIfCancellationRequested();
            var backupFileInfo = new FileInfo(Path.Combine(_currentBackupDirectory, $"{sigFile.Sha256}.{_appSettings.CompressFileExtension}"));
            var sourceFileInfo = new FileInfo(sigFile.Path);
            FileInfo sigFileInfo = null;

            try
            {
                var existentSigFiles = FindExistentSigFiles(sigFile);

                if (!existentSigFiles.Any())
                {
                    _logger.Trace($"Compress file: {sourceFileInfo.FullName} -> {backupFileInfo.FullName}");

                    if (_compressCli.Compress(sourceFileInfo, backupFileInfo))
                    {
                        using (var sigFileCreator = new SigFileCreator(_currentBackupDirectory, _cancel))
                        {
                            sigFileInfo = await sigFileCreator.Create(sigFile).ConfigureAwait(false);
                        }

                        Interlocked.Increment(ref _filesProcessed);
                    }
                    else
                    {
                        Interlocked.Increment(ref _filesSkipped);
                        _logger.Error($"Compress file failed: {sourceFileInfo.FullName}");
                    }
                }
                else
                {
                    await HandleExistentBackupFiles(existentSigFiles, sigFile, backupFileInfo)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                sigFileInfo?.DeleteIfExists();
                backupFileInfo.DeleteIfExists();

                if (!_fileExceptionHandler.CanSkip(ex)) throw;

                _logger.Warn($"File access failed: {ex.Message}");
                Interlocked.Increment(ref _filesSkipped);
            }
        }

        async Task HandleExistentBackupFiles(IEnumerable<string> existentSigFiles, SigFileInfo sigFile, FileInfo backupFileInfo)
        {
            var sourceFilePath = sigFile.Path;

            foreach (var existingSigFile in existentSigFiles)
            {
                if (await HasSigFileIncluded(existingSigFile, sourceFilePath).ConfigureAwait(false))
                {
                    _logger.Trace($"File already backed up, skip file: {sourceFilePath}");
                    Interlocked.Increment(ref _filesSkipped);
                    return;
                }
            }

            var existentBackupFile = new FileInfo(Path.ChangeExtension(existentSigFiles.First(), _appSettings.CompressFileExtension));

            if (!existentBackupFile.FullName.Equals(backupFileInfo.FullName, StringComparison.OrdinalIgnoreCase) &&
                !backupFileInfo.Exists)
            {
                _logger.Trace($"Backup exists but missing signature, copy file: {sourceFilePath} -> {backupFileInfo.FullName}");
                await existentBackupFile.Copy(backupFileInfo.FullName, _cancel).ConfigureAwait(false);
            }

            using (var sigFileCreator = new SigFileCreator(_currentBackupDirectory, _cancel))
            {
                await sigFileCreator.Create(sigFile).ConfigureAwait(false);
            }

            Interlocked.Increment(ref _filesProcessed);
        }

        async Task<bool> HasSigFileIncluded(string sigFilePath, string file)
        {
            if (!File.Exists(sigFilePath)) return false;
            using (var reader = new BatchFileReader(sigFilePath))
            {
                IEnumerable<string> items;
                bool hasFile = false;

                while (!hasFile && (items = await reader.Take(100).ConfigureAwait(false)).Any())
                {
                    hasFile = items
                        .Select(i => SigFileInfo.Parse(i))
                        .Where(i => i != null && i.Path.Equals(file, StringComparison.OrdinalIgnoreCase))
                        .Any();
                }

                return hasFile;
            }
        }
    }
}
