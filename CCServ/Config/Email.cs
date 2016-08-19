﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Config
{
    public static class Email
    {
        public static System.Net.Mail.MailAddress DeveloperDistroAddress
        {
            get
            {
                return new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications");
            }
        }

        public static System.Net.Mail.MailAddress AtwoodAddress
        {
            get
            {
                return new System.Net.Mail.MailAddress("sundevilgoalie13@gmail.com", "Programmer Extraordinaire");
            }
        }

        public static System.Net.Mail.MailAddress McLean
        {
            get
            {
                return new System.Net.Mail.MailAddress("anguslmm@gmail.com", "Angus McLean");
            }
        }

        public static string DODEmailHost
        {
            get
            {
                return "mail.mil";
            }
        }
    }
}