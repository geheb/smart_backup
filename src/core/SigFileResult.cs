namespace geheb.smart_backup.core
{
    internal sealed class SigFileResult
    {
        public string FilePath { get; }
        public long FilesChecked { get; }

        public SigFileResult(string filePath, long filesChecked)
        {
            FilePath = filePath;
            FilesChecked = filesChecked;
        }
    }
}
