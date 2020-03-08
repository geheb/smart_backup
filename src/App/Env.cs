using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Geheb.SmartBackup.App
{
    class Env
    {
        public string CurrentProcessDirectory { get; }

        public Env()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            CurrentProcessDirectory = Path.GetDirectoryName(processModule?.FileName);
        }
    }
}
