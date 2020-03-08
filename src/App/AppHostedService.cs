using Geheb.SmartBackup.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Geheb.SmartBackup.App
{
    class AppHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IAppCommand _appCommand;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingToken = new CancellationTokenSource();

        public AppHostedService(
            ILogger<AppHostedService> logger,
            IAppCommand appCommand)
        {
            _logger = logger;
            _appCommand = appCommand;
        }

        public void Dispose()
        {
            _stoppingToken.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = _appCommand.ExecuteAsync(_stoppingToken.Token);
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingToken.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }
    }
}
