using System;
using System.Collections.Generic;

namespace geheb.smart_backup.cli
{
    internal sealed class BackupArgs
    {
        public List<string> File { get; set; }

        public string Target { get; set; }

        public List<string> IgnoreRegexPattern { get; set; }

        public string Password { get; set; }

        public int MaxBackupSets { get; set; }
    }
}
