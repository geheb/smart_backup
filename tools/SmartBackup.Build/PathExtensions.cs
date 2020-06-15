using System.Diagnostics;
using System.IO;

namespace Geheb.SmartBackup.Build
{
    static class PathExtensions
    {
        public static void CleanDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;

            foreach (var file in Directory.EnumerateFiles(directory))
            {
                File.Delete(file);
            }

            foreach (var dir in Directory.EnumerateDirectories(directory))
            {
                Directory.Delete(dir, true);
            }
        }

        public static string ExpandPath(string path)
        {
            if (Debugger.IsAttached)
            {
                return Path.GetFullPath(Path.Combine("../../../../../", path));
            }
            return Path.GetFullPath(path);
        }

        public static void CopyAll(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            foreach (string dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relSourcePath = dir.Substring(sourceDir.Length);
                Directory.CreateDirectory(Path.Combine(targetDir, relSourcePath.TrimStart('/', '\\')));
            }

            foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relSourcePath = file.Substring(sourceDir.Length);
                File.Copy(file, Path.Combine(targetDir, relSourcePath.TrimStart('/', '\\')));
            }
        }
    }
}
