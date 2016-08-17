using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Args
{
    public class BeginPasswordResetEmailArgs : BaseEmailArgs
    {
        public Guid PasswordResetId { get; set; }

        public string FriendlyName { get; set; }
    }
}
