using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geheb.smart_backup
{
    internal enum ExitCode
    {
        Success,
        Cancelled,
        ArgumentError,
        InternalError,
        NotImplemented,
        InvalidAppSettings
    };
}
