using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geheb.smart_backup.cli
{
    sealed class BackupArgs
    {
        public List<string> File { get; set; }

        public string Target { get; set; }

        public List<string> IgnoreRegexPattern { get; set; }

        public string Password { get; set; }

        public int HistoryCount { get; set; }
    }
}
