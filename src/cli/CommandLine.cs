using Fclp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace geheb.smart_backup.cli
{
    sealed class CommandLine
    {
        readonly string _app;
        readonly Version _fileVersion;
        readonly FileVersionInfo _fileInfo;
        public BackupArgs Backup { get; private set; }

        public CommandLine()
        {
            var asm = Assembly.GetExecutingAssembly();
            _app = asm.GetName().Name;
            _fileVersion = asm.GetName().Version;
            _fileInfo = FileVersionInfo.GetVersionInfo(asm.Location);
        }

        void PrintHeader()
        {
            Console.WriteLine($"{_app} v{_fileVersion}");
            Console.WriteLine(_fileInfo.LegalCopyright);
            Console.WriteLine();
        }

        void PrintBaseHelp()
        {
            Console.WriteLine($"Usage: {_app} [--help] [--version] <command> [<args>]");
            Console.WriteLine();
            Console.WriteLine("These are common commands used in various situations:");
            Console.WriteLine("   backup       backup files and folders into specified directory");
            Console.WriteLine("   find         find backed up file");
            Console.WriteLine();
            Console.WriteLine($"See also {_app} <command> --help");
        }

        public bool Parse(string[] args)
        {
            PrintHeader();

            if (args.Length < 1 || args[0].Equals("--help", StringComparison.OrdinalIgnoreCase))
            {
                PrintBaseHelp();
                return false;
            }

            if (args[0].Equals("--version", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(_fileVersion);
                return false;
            }

            if (!args[0].Equals("backup", StringComparison.OrdinalIgnoreCase))
            {
                PrintBaseHelp();
                return false;
            }

            var parser = new FluentCommandLineParser<BackupArgs>();
            parser.SetupHelp("?", "help")
                .Callback(text => Console.WriteLine(text));

            parser.Setup(arg => arg.File)
                .As('f', "file")
                .Required()
                .WithDescription("Files and directories to backup");

            parser.Setup(arg => arg.Target)
                .As('t', "target")
                .Required()
                .WithDescription("Directory where to place backup files");

            parser.Setup(arg => arg.Password)
                .As('p', "password")
                .Required()
                .WithDescription("Password to encrypt backed up files");

            parser.Setup(arg => arg.IgnoreRegexPattern)
                .As('i', "ignore")
                .WithDescription("Which files should be ignored (use regex pattern)");

            parser.Setup(arg => arg.MaxBackupSets)
                .As('m', "maxbackupsets")
                .WithDescription("How many backup sets should be created");

            var result = parser.Parse(args.Skip(1).ToArray());
            if (result.HelpCalled) return false;

            if (result.HasErrors)
            {
                Console.Error.WriteLine(result.ErrorText);
                return false;
            }

            Backup = parser.Object;
            return true;
        }
    }
}
