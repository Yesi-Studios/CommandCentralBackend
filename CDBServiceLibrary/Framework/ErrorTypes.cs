using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedServiceFramework.Framework
{
    public enum ErrorTypes
    {
        Validation,
        Authorization,
        Authentication,
        LockOwned,
        LockImpossible,
        Fatal,
        NULL
    }
}
