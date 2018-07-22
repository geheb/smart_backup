using geheb.smart_backup.io;
using geheb.smart_backup.text;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace geheb.smart_backup.core
{
    sealed class CompressCli
    {
        readonly InlineIfExpressionParser _compressArgsParser;
        readonly string _password;
        readonly CancellationToken _cancel;
        readonly AppSettings _appSettings;

        public CompressCli(AppSettings appSettings, string password, CancellationToken cancel)
        {
            _appSettings = appSettings;
            _password = password;
            _cancel = cancel;
            _compressArgsParser = InlineIfExpressionParser.Parse(_appSettings.CompressArguments);
        }

        public bool Compress(FileInfo targetFile, FileInfo sourceFile)
        {
            string args = _compressArgsParser != null ?
                _compressArgsParser.Calc("sourcefilelength", sourceFile.Length) :
                _appSettings.CompressArguments;

            args = args
                .Replace("{password}", _password.SurroundWithQuotationMarks(), StringComparison.OrdinalIgnoreCase)
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
                        if (_cancel.IsCancellationRequested)
                        {
                            try
                            {
                                proc.Kill();
                            }
                            finally
                            {
                                _cancel.ThrowIfCancellationRequested();
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
