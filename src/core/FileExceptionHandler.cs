using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace geheb.smart_backup.core
{
    internal sealed class FileExceptionHandler
    {
        public bool CanSkip(Exception ex)
        {
            var isSharingViolation = (Marshal.GetHRForException(ex) & 0xffff) == 32;
            if (isSharingViolation 
                || ex is UnauthorizedAccessException
                || ex is IOException)
            {
                return true;
            }
            return false;
        }
    }
}
