using System;
using System.Threading;

namespace geheb.smart_backup.core
{
    internal sealed class ShutdownHandler : IShutdownHandler
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public CancellationToken Token => _cancellationTokenSource.Token;

        public ShutdownHandler()
        {
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _cancellationTokenSource.Cancel();
            };
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}
