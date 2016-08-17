using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.Email.Args;

namespace CCServ.Email
{
    /// <summary>
    /// The begin registration error email.
    /// </summary>
    public class BeginRegistrationErrorEmail : CCEmailMessage
    {
        /// <summary>
        /// The template of this email.
        /// </summary>
        public override string Template
        {
            get
            {
                return Templates.TemplateManager.AllTemplates["BeginRegistrationErrorEmail.html"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public BeginRegistrationErrorEmail(BeginRegistrationErrorEmailArgs args) : base(args)
        {
        }
    }
}
