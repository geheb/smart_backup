using geheb.smart_backup.cli;
using geheb.smart_backup.core;
using NLog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace geheb.smart_backup
{
    internal sealed class Program
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            using (var shutdownHandler = new ShutdownHandler())
            {
                try
                {
                    var program = new Program();
                    return (int)program.Main(args, shutdownHandler)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (AggregateException ex)
                {
                    try
                    {
                        ex.Handle(e => e is OperationCanceledException);
                        return (int)ExitCode.Cancelled;
                    }
                    catch (Exception ex2)
                    {
                        _logger.Error(ex2);
                        return (int)ExitCode.InternalError;
                    }
                }
                finally
                {
                    if (Environment.UserInteractive)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey();
                    }
                }
            }
        }

        async Task<ExitCode> Main(string[] args, IShutdownHandler shutdownHandler)
        {
            var appSettings = AppSettings.Load(Console.Error);
            if (appSettings == null || !appSettings.Validate(Console.Error))
            {
                return ExitCode.InvalidAppSettings;
            }

            var cli = new CommandLine();
            if (!cli.Parse(args))
            {
                return ExitCode.ArgumentError;
            }

            if (cli.Backup != null)
            {
                var startTime = Stopwatch.StartNew();
                try
                {
                    using (var backup = new BackupCreator(appSettings, shutdownHandler, cli.Backup))
                    {
                        await backup.Create().ConfigureAwait(false);
                    }
                    return ExitCode.Success;
                }
                catch (OperationCanceledException)
                {
                    _logger.Warn("Operation canceled");
                    throw;
                }
                finally
                {
                    startTime.Stop();
                    _logger.Info($"Elapsed time: {startTime.Elapsed}");
                }
            }

            return ExitCode.NotImplemented;
        }
    }
}
