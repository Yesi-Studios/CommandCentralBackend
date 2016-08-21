using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Models
{
    public class CriticalMessageEmailModel
    {
        //(string message, MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)

        public string Message { get; set; }

        public ClientAccess.MessageToken Token { get; set; }

        public string CallerMemberName { get; set; }

        public int CallerLineNumber { get; set; }

        public string CallerFilePath { get; set; }

    }
}
