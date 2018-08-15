using geheb.smart_backup.cli;
using geheb.smart_backup.core;
using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace geheb.smart_backup
{
    sealed class Program
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        enum ExitCode { Success, Cancelled, ArgumentError, InternalError, NotImplemented, InvalidAppSettings };

        static int Main(string[] args)
        {
            using (var cancel = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cancel.Cancel();
                };

                try
                {
                    var program = new Program();
                    return (int)program.Main(args, cancel.Token)
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

        async Task<ExitCode> Main(string[] args, CancellationToken cancel)
        {
            var appSettings = AppSettings.Load();
            if (appSettings == null || !appSettings.Validate())
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
                    using (var backup = new BackupCreator(appSettings, cancel, cli.Backup))
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
