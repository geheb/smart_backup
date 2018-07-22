using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geheb.smart_backup.io
{
    static class FileFactory
    {
        public static FileStream OpenAndAppendAsFileStream(string path)
        {
            return new FileStream(path,
                FileMode.Append, FileAccess.Write, FileShare.Read,
                4096, FileOptions.Asynchronous | FileOptions.WriteThrough);
        }

        public static FileStream CreateAsFileStream(string path)
        {
            return new FileStream(path,
                FileMode.Create, FileAccess.Write, FileShare.Read,
                4096, FileOptions.Asynchronous | FileOptions.WriteThrough);
        }

        public static FileStream OpenAsFileStream(string path)
        {
            return new FileStream(path,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }
    }
}
