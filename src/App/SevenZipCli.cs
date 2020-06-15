using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;
using System.IO.Abstractions;

namespace Geheb.SmartBackup.App
{
    class SevenZipCli
    {
        private readonly string _cliPath;
        private readonly string[] _ignoreCmpressionForFileExtension = new[] { ".7z", ".zip", ".rar", ".jpg" };

        public SevenZipCli(Env env, IFileSystem fileSystem)
        {
            _cliPath = Environment.Is64BitProcess ?
                fileSystem.Path.Combine(env.CurrentProcessDirectory, "7zip", "x64", "7za.exe") :
                fileSystem.Path.Combine(env.CurrentProcessDirectory, "7zip", "7za.exe");
        }

        public void Compress(IFileInfo sourceFile, IFileInfo targetFile, string password, CancellationToken cancellationToken)
        {
            var args = BuildAddParam(sourceFile, targetFile, password);

            var procInfo = new ProcessStartInfo(_cliPath, args)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false
            };

            using (var proc = Process.Start(procInfo))
            {
                if (proc == null) return;
                while (!proc.WaitForExit(5000))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            proc.Kill();
                        }
                        finally
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                }
                if (proc.ExitCode != 0) throw new IOException("7zip failed, invalid exit code: " + proc.ExitCode);
            }
        }

        private string BuildAddParam(IFileInfo sourceFile, IFileInfo targetFile, string password)
        {
            var split = sourceFile.Length > (1000 * 1000 * 200) ? " -v100M" : string.Empty; // split into 100 mb

            var compressLevel = "-mx=0"; // no compression
            if (sourceFile.Length > 1000)
            {
                compressLevel = _ignoreCmpressionForFileExtension.Any(e => sourceFile.Extension.EndsWith(e, StringComparison.OrdinalIgnoreCase)) ?
                    "-mx=1" : // fast compressing
                    "-mx=5";  // normal compressing
            }

            // -mtr=off = store no file attributes. 
            // -mtc=on = add creation timestamps for files.
            // -mhe=on = eneble archive header encryption. 

            return $"a -p{password}{split} {compressLevel} -mtr=off -mtc=on -mhe=on -spf2 \"{targetFile.FullName}\" \"{sourceFile.FullName}\"";
        }
    }
}
