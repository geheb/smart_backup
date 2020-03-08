using System.Collections.Generic;
using System.Linq;

namespace Geheb.SmartBackup.Models
{
    internal class BackupParam : IParam
    {
        public string[] SourceDir { get; }
        public string TargetDir { get; }
        public string Password { get; }
        public int MaxBackupSets { get; }

        public BackupParam(IEnumerable<string> values, string targetDir, string password, int maxBackupSets)
        {
            SourceDir = values.ToArray();
            TargetDir = targetDir;
            Password = password;
            MaxBackupSets = maxBackupSets;
        }
    }
}