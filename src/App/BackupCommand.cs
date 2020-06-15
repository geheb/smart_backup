using Geheb.SmartBackup.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Geheb.SmartBackup.App
{
    class BackupCommand : IAppCommand
    {
        private const string ReferenceFileExtension = ".sbref";
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger _logger;
        private readonly SHA256Generator _hashGenerator;
        private readonly BackupParam _backupParam;
        private readonly RecursiveFileEnumerator _recursiveFileEnumerator;
        private readonly SevenZipCli _sevenZipCli;
        private readonly IFileSystem _fileSystem;
        private readonly object _summaryFileLock = new object();
        private readonly ConcurrentDictionary<string, object> _fileLocks = new ConcurrentDictionary<string, object>();
        private string[] _oldBackupDirectories;

        public BackupCommand(
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<BackupCommand> logger,
            SHA256Generator hashGenerator,
            BackupParam backupParam,
            RecursiveFileEnumerator recursiveFileEnumerator,
            SevenZipCli sevenZipCli,
            IFileSystem fileSystem)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
            _hashGenerator = hashGenerator;
            _backupParam = backupParam;
            _recursiveFileEnumerator = recursiveFileEnumerator;
            _sevenZipCli = sevenZipCli;
            _fileSystem = fileSystem;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _oldBackupDirectories = CleanupOldBackupDirectories();

            var start = Stopwatch.StartNew();
            var backupDirName = DateTime.UtcNow.ToString("o").Replace(':', '-');
            var summaryFile = _fileSystem.Path.Combine(_backupParam.TargetDir, backupDirName, "summary" + ReferenceFileExtension);
            var backupDir = _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(_backupParam.TargetDir, backupDirName));
            long count = 0;
            try
            {
                foreach (var dir in _backupParam.SourceDir)
                {
                    foreach (var files in _recursiveFileEnumerator.Enumerate(dir, Environment.ProcessorCount))
                    {
                        count += files.Length;
                        _fileLocks.Clear();
                        var tasks = from file in files select Task.Run(() => BackupFiles(_fileSystem.FileInfo.FromFileName(file), backupDirName, summaryFile, cancellationToken), cancellationToken);
                        await Task.WhenAll(tasks);
                    }
                }
            }
            finally
            {
                start.Stop();
                _logger.LogInformation($"Files processed: {count}, elapsed time: {start.Elapsed}");
                if (count < 1) backupDir.Delete();
                _hostApplicationLifetime.StopApplication();

            }
        }

        private bool HasBackupFile(string sourceFileHash)
        {
            if (_oldBackupDirectories == null || _oldBackupDirectories.Length < 1) return false;

            foreach (var backupDir in _oldBackupDirectories)
            {
                var baseFile = _fileSystem.Path.Combine(backupDir, sourceFileHash.Substring(0, 2), sourceFileHash);
                if (_fileSystem.File.Exists(baseFile + ".7z") || _fileSystem.File.Exists(baseFile + ".7z.001")) return true;
            }

            return false;
        }

        private Task BackupFiles(IFileInfo sourceFile, string backupDirName, string summaryFile, CancellationToken cancellationToken)
        {
            IFileInfo targetFile = null;
            try
            {
                _logger.LogDebug($"Create SHA256 from file '{sourceFile.FullName}' ...");
                var sourceFileHash = _hashGenerator.GenerateFrom(sourceFile);
                _logger.LogDebug($"SHA256 created: '{sourceFile.FullName}' -> {sourceFileHash}");

                if (HasBackupFile(sourceFileHash))
                {
                    _logger.LogInformation($"File already backed up: '{sourceFile.FullName}'");
                    lock (_summaryFileLock)
                    {
                        AddFileInfo(summaryFile, sourceFile, sourceFileHash);
                    }
                    return Task.CompletedTask;
                }

                var fileLock = _fileLocks.GetOrAdd(sourceFileHash, f => new object());

                lock (fileLock)
                {
                    var backupDir = _fileSystem.Path.Combine(_backupParam.TargetDir, backupDirName, sourceFileHash.Substring(0, 2));
                    if (CreateReferenceIfFileExists(sourceFile, backupDir, sourceFileHash))
                    {
                        _logger.LogInformation($"Hash already exists, reference created: '{sourceFile.FullName}'");
                        lock (_summaryFileLock)
                        {
                            AddFileInfo(summaryFile, sourceFile, sourceFileHash);
                        }
                        return Task.CompletedTask;
                    }

                    targetFile = _fileSystem.FileInfo.FromFileName(_fileSystem.Path.Combine(backupDir, sourceFileHash + ".7z"));
                    targetFile.Directory.Create();

                    _logger.LogDebug($"Create backup from file '{sourceFile.FullName}' ...");
                    _sevenZipCli.Compress(sourceFile, targetFile, _backupParam.Password, cancellationToken);
                }
                
                _logger.LogInformation($"Backup created: '{sourceFile.FullName}' -> '{targetFile.FullName}'");
                lock (_summaryFileLock)
                {
                    AddFileInfo(summaryFile, sourceFile, sourceFileHash);
                }
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, $"Create backup failed: '{sourceFile.FullName}'");
                if (targetFile != null && targetFile.Exists) targetFile.Delete();
            }
            return Task.CompletedTask;
        }

        private bool CreateReferenceIfFileExists(IFileInfo sourceFile, string backupDir, string sourceFileHash)
        {
            if (!_fileSystem.Directory.Exists(backupDir)) return false;

            if (!_fileSystem.File.Exists(_fileSystem.Path.Combine(backupDir, sourceFileHash + ".7z")) &&
                !_fileSystem.File.Exists(_fileSystem.Path.Combine(backupDir, sourceFileHash + ".7z.001"))) return false;

            AddFileInfo(_fileSystem.Path.Combine(backupDir, sourceFileHash + ReferenceFileExtension), sourceFile, sourceFileHash);
            return true;
        }

        private void AddFileInfo(string summaryFile, IFileInfo fileInfo, string hash)
        {
            _fileSystem.File.AppendAllText(summaryFile,
                fileInfo.FullName + "\t" +
                fileInfo.LastWriteTimeUtc.ToString("o") + "\t" +
                fileInfo.Length + "\t" +
                hash + "\n");
        }

        private string[] CleanupOldBackupDirectories()
        {
            if (_backupParam.MaxBackupSets < 1) return Array.Empty<string>();

            var currentBackupDirectories = new SortedDictionary<string, byte>(new DescendedStringComparer(StringComparison.OrdinalIgnoreCase));

            foreach (var dir in _fileSystem.Directory.EnumerateDirectories(_backupParam.TargetDir, "*", SearchOption.TopDirectoryOnly))
            {
                currentBackupDirectories.Add(dir, 0);
            }

            if (currentBackupDirectories.Count <= _backupParam.MaxBackupSets) return currentBackupDirectories.Keys.ToArray();

            foreach (var dir in currentBackupDirectories.Keys.Skip(_backupParam.MaxBackupSets))
            {
                _fileSystem.Directory.Delete(dir, true);
            }

            return currentBackupDirectories.Keys.Take(_backupParam.MaxBackupSets).ToArray();
        }
    }
}
