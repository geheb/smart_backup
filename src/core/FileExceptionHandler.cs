using System;
using System.IO;
using System.Runtime.InteropServices;

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
