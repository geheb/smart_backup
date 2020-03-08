using System;
using System.Collections.Generic;
using System.Linq;

namespace Geheb.SmartBackup.Build
{
    static class CommandLineExtensions
    {
        static readonly string[] SupportedArgs = new[] { "--_semver=", "--_githash=" };
        public static string GetSemVer(string[] args)
        {
            var verArg = SupportedArgs[0];
            var arg = args.FirstOrDefault(a => a.StartsWith(verArg, StringComparison.OrdinalIgnoreCase));
            return arg != null ? arg.Substring(verArg.Length) : "1.0.0";
        }

        public static string GetGitHash(string[] args)
        {
            var gitHashArg = SupportedArgs[1];
            var arg = args.FirstOrDefault(a => a.StartsWith(gitHashArg, StringComparison.OrdinalIgnoreCase));
            return arg != null ? arg.Substring(gitHashArg.Length) : string.Empty;
        }

        public static string GetSemVerWithGitHash(string[] args)
        {
            var semVer = GetSemVer(args);
            var gitHash = GetGitHash(args);
            return semVer + (gitHash != null ? "-" + gitHash : string.Empty);
        }

        public static string[] CleanupArgs(string[] args)
        {
            return args.Where(a => !SupportedArgs.Any(s => a.StartsWith(s, StringComparison.OrdinalIgnoreCase))).ToArray();
        }
    }
}
