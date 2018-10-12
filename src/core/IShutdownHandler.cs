using System;
using System.Threading;

namespace geheb.smart_backup.core
{
    internal interface IShutdownHandler : IDisposable
    {
        CancellationToken Token { get; }
    }
}
