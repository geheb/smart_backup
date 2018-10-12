using Newtonsoft.Json;
using System;
using System.IO;

namespace geheb.smart_backup.core
{
    internal sealed class AppSettings
    {
        private const string FileName = "appSettings.json";

        public string CompressApp { get; set; }
        public string CompressFileExtension { get; set; }
        public string CompressArguments { get; set; }

        public bool Validate(TextWriter errorWriter)
        {
            if (string.IsNullOrEmpty(CompressApp))
            {
                errorWriter.WriteLine($"missing compression app in {FileName}");
                return false;
            }

            if (!File.Exists(CompressApp))
            {
                errorWriter.WriteLine($"compression app not found: {CompressApp}");
                return false;
            }

            if (string.IsNullOrEmpty(CompressFileExtension))
            {
                errorWriter.WriteLine($"missing compression file extension in {FileName}");
                return false;
            }

            if (string.IsNullOrEmpty(CompressArguments))
            {
                errorWriter.WriteLine($"missing compression arguments in {FileName}");
            }

            return true;
        }

        public static AppSettings Load(TextWriter errorWriter)
        {
            var fi = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName));
            if (!fi.Exists)
            {
                errorWriter.WriteLine($"file not found: {FileName}");
                return null;
            }

            var json = File.ReadAllText(fi.FullName);

            var settings = JsonConvert.DeserializeObject<AppSettings>(json);

            if (!string.IsNullOrEmpty(settings?.CompressApp))
            {
                settings.CompressApp = Path.GetFullPath(settings.CompressApp);
            }

            return settings;
        }
    }
}
