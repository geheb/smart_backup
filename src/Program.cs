using Geheb.SmartBackup.App;
using Geheb.SmartBackup.Models;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Geheb.SmartBackup
{
    class Program
    {
        private const string _appsettingsFile = "appsettings.json";
        private const string _hostsettingsFile = "hostsettings.json";
        private const string _environmentPrefix = "SMARTBACKUP_";

        static async Task<int> Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "SmartBackup",
                Description = "A simple and secure backup tool"
            };

            app.HelpOption(inherited: true);

            app.Command("backup", backupCmd =>
            {
                backupCmd.Description = "Create a backup of specified directory";

                var sourceDirOption = backupCmd
                    .Option("-s|--source-dir <PATH>", "Source directory to create backup", CommandOptionType.MultipleValue)
                    .IsRequired()
                    .Accepts(v => v.ExistingDirectory());

                var targetDirOption = backupCmd
                    .Option("-t|--target-dir <PATH>", "Target directory to put backup", CommandOptionType.SingleValue)
                    .IsRequired()
                    .Accepts(v => v.LegalFilePath());

                var passwordOption = backupCmd
                    .Option("-p|--password <PASSWORD>", "Password to encrypt backup files", CommandOptionType.SingleValue);

                var maxBackupSetsOption = backupCmd
                    .Option<int>("-m|--max-backup <COUNT>", "Keep max backup sets and delete the oldest ones", CommandOptionType.SingleValue)
                    .Accepts(v => v.Range(0, 1000));

                backupCmd.OnExecuteAsync((cancellationToken) =>
                {
                    var param = new BackupParam(sourceDirOption.Values, targetDirOption.Value(), passwordOption.Value(), maxBackupSetsOption.ParsedValue);
                    return RunHostAsync<BackupCommand, BackupParam>(args, param, cancellationToken);
                });
            });

            app.VersionOption("-v|--version", typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

            app.OnExecute(() =>
            {
                app.Error.WriteLine("Specify a subcommand");
                app.ShowHelp();
                return 1;
            });

            try
            {
                return await app.ExecuteAsync(args);
            }
            catch (CommandParsingException ex)
            {
                app.Error.WriteLine(ex.Message);
                return -1;
            }
        }

        private static async Task<int> RunHostAsync<TCommand, TParam>(string[] args, TParam param, CancellationToken cancellationToken)
            where TCommand : class, IAppCommand
            where TParam : class, IParam
        {
            var env = new Env();
            var host = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(env.CurrentProcessDirectory);
                    configHost.AddJsonFile(_hostsettingsFile, optional: true);
                    configHost.AddEnvironmentVariables(prefix: _environmentPrefix);
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(env.CurrentProcessDirectory);
                    configApp.AddJsonFile(_appsettingsFile, optional: true);
                    configApp.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                    configApp.AddEnvironmentVariables(prefix: _environmentPrefix);
                    configApp.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                    services.Configure<Application>(hostContext.Configuration.GetSection("application"));
                    services.AddHostedService<AppHostedService>();
                    services.AddSingleton(param);
                    services.AddSingleton<IAppCommand, TCommand>();
                    services.AddTransient<SHA256Generator>();
                    services.AddTransient<RecursiveFileEnumerator>();
                    services.AddTransient<SevenZipCli>();
                    services.AddTransient<IFileSystem, FileSystem>();
                    services.AddSingleton(env);
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    Log.Logger = new LoggerConfiguration()
                      .ReadFrom.Configuration(hostContext.Configuration)
                      .CreateLogger();

                    configLogging.AddSerilog(dispose: true);
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync(cancellationToken);
            return 0;
        }
    }
}
