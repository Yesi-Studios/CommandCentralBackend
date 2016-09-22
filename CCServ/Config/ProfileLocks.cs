using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Config
{
    public static class ProfileLocks
    {
        public static TimeSpan MaxAge
        {
            get
            {
                return TimeSpan.FromMinutes(20);
            }
        }
    }
}
