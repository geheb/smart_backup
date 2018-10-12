using geheb.smart_backup.io;
using geheb.smart_backup.text;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace geheb.smart_backup.core
{
    internal sealed class CompressCli
    {
        private readonly InlineIfExpressionParser _compressArgsParser;
        private readonly CancellationToken _cancellationToken;
        private readonly AppSettings _appSettings;

        public CompressCli(AppSettings appSettings, IShutdownHandler shutdownHandler)
        {
            _appSettings = appSettings;
            _cancellationToken = shutdownHandler.Token;
            _compressArgsParser = InlineIfExpressionParser.Parse(_appSettings.CompressArguments);
        }

        public bool Compress(FileInfo sourceFile, FileInfo targetFile, string password)
        {
            string args = _compressArgsParser != null ?
                _compressArgsParser.Calc("sourcefilelength", sourceFile.Length) :
                _appSettings.CompressArguments;

            args = args
                .Replace("{password}", password.SurroundWithQuotationMarks(), StringComparison.OrdinalIgnoreCase)
                .Replace("{targetfile}", targetFile.FullName.SurroundWithQuotationMarks(), StringComparison.OrdinalIgnoreCase)
                .Replace("{sourcefile}", sourceFile.FullName.SurroundWithQuotationMarks(), StringComparison.OrdinalIgnoreCase);

            var procInfo = new ProcessStartInfo(_appSettings.CompressApp, args)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            bool hasValidExitCode = false;
            try
            {
                using (var proc = Process.Start(procInfo))
                {
                    if (proc == null) return false;
                    while (!proc.WaitForExit(1000))
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                proc.Kill();
                            }
                            finally
                            {
                                _cancellationToken.ThrowIfCancellationRequested();
                            }
                        }
                    }
                    hasValidExitCode = proc.ExitCode == 0;
                }
            }
            finally
            {
                if (!hasValidExitCode)
                {
                    targetFile.DeleteIfExists();
                }
            }
            return hasValidExitCode;
        }
    }
}
