using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Geheb.SmartBackup.Models
{
    interface IAppCommand
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
