using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geheb.smart_backup.core
{
    sealed class SigFileInfo
    {
        const char Separator = '\t';
        public string Path { get; private set; }
        public long Length { get; private set; }
        public DateTime CreationTimeUtc { get; private set; }
        public string Sha256 { get; private set; }

        public bool IsFileModified
        {
            get
            {
                var fi = new FileInfo(Path);
                if (!fi.Exists) return false;
                return Length != fi.Length ||
                    !CreationTimeUtc.Equals(fi.CreationTimeUtc);
            }
        }

        public SigFileInfo(FileInfo fi, string sha256)
        {
            Path = fi.FullName;
            Length = fi.Length;
            CreationTimeUtc = fi.CreationTimeUtc;
            Sha256 = sha256;
        }

        public void Update(FileInfo fi, string sha256)
        {
            Path = fi.FullName;
            Length = fi.Length;
            CreationTimeUtc = fi.CreationTimeUtc;
            Sha256 = sha256;
        }

        private SigFileInfo(string path, long length, DateTime time, string sha256)
        {
            Path = path;
            Length = length;
            CreationTimeUtc = time;
            Sha256 = sha256;
        }

        public static SigFileInfo Parse(string line)
        {
            var items = line.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length != 4)
            {
                throw new InvalidDataException("Invalid sig info: " + line);
            }

            if (items[0].Length < 2 ||
                !long.TryParse(items[1], out var length) ||
                !DateTime.TryParse(items[2], out var time) ||
                items[3].Length != 64)
            {
                throw new InvalidDataException("Invalid sig info: " + line);
            }

            return new SigFileInfo(items[0], length, time, items[3]);
        }

        public string Serialize()
        {
            return 
                Path + Separator +
                Length + Separator +
                CreationTimeUtc.ToString("o") + Separator +
                Sha256 + Environment.NewLine;
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
