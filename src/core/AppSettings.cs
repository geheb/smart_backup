using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geheb.smart_backup.core
{
    sealed class AppSettings
    {
        public string CompressApp { get; set; }
        public string CompressFileExtension { get; set; }
        public string CompressArguments { get; set; }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(CompressApp))
            {
                Console.Error.WriteLine("missing compression app in appSettings.json");
                return false;
            }

            if (!File.Exists(CompressApp))
            {
                Console.Error.WriteLine($"compression app not found: {CompressApp}");
                return false;
            }

            if (string.IsNullOrEmpty(CompressFileExtension))
            {
                Console.Error.WriteLine("missing compression file extension in appSettings.json");
                return false;
            }

            if (string.IsNullOrEmpty(CompressArguments))
            {
                Console.Error.WriteLine("missing compression arguments in appSettings.json");
            }

            return true;
        }

        public static AppSettings Load()
        {
            var fi = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json"));
            if (!fi.Exists)
            {
                Console.Error.WriteLine("missing file appSettings.json");
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
