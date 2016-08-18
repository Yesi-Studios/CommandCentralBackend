using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.Email.Args;

namespace CCServ.Email
{
    public class BeginPasswordResetEmail : CCEmailMessage
    {
        public BeginPasswordResetEmail(BeginPasswordResetEmailArgs args) : base(args)
        {
        }

        public override string Template
        {
            get
            {
                return "BeginPasswordResetEmail.html";
            }
        }
    }
}
