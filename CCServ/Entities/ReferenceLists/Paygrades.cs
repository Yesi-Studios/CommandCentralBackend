using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.ReferenceLists
{
    public static class Paygrades
    {

        static Paygrades()
        {
            var paygrades = typeof(Paygrades).GetFields().Where(x => x.FieldType == typeof(Paygrade)).Select(x => (Paygrade)x.GetValue(null)).ToList();

            AllPaygrades = new ConcurrentBag<Paygrade>(paygrades);
        }

        public static ConcurrentBag<Paygrade> AllPaygrades;

        public static Paygrade E1 = new Paygrade { Value = "E1", Description = "" };
        public static Paygrade E2 = new Paygrade { Value = "E2", Description = "" };
        public static Paygrade E3 = new Paygrade { Value = "E3", Description = "" };
        public static Paygrade E4 = new Paygrade { Value = "E4", Description = "" };
        public static Paygrade E5 = new Paygrade { Value = "E5", Description = "The best rank in the Navy." };
        public static Paygrade E6 = new Paygrade { Value = "E6", Description = "" };
        public static Paygrade E7 = new Paygrade { Value = "E7", Description = "" };
        public static Paygrade E8 = new Paygrade { Value = "E8", Description = "" };
        public static Paygrade E9 = new Paygrade { Value = "E9", Description = "" };
        public static Paygrade CWO2 = new Paygrade { Value = "CWO2", Description = "" };
        public static Paygrade CWO3 = new Paygrade { Value = "CWO3", Description = "" };
        public static Paygrade CWO4 = new Paygrade { Value = "CWO4", Description = "" };
        public static Paygrade CWO5 = new Paygrade { Value = "CWO5", Description = "" };
        public static Paygrade O1 = new Paygrade { Value = "O1", Description = "" };
        public static Paygrade O1E = new Paygrade { Value = "O1E", Description = "" };
        public static Paygrade O2E = new Paygrade { Value = "O2E", Description = "" };
        public static Paygrade O3E = new Paygrade { Value = "O3E", Description = "" };
        public static Paygrade O2 = new Paygrade { Value = "O2", Description = "" };
        public static Paygrade O3 = new Paygrade { Value = "O3", Description = "" };
        public static Paygrade O4 = new Paygrade { Value = "O4", Description = "" };
        public static Paygrade O5 = new Paygrade { Value = "O5", Description = "" };
        public static Paygrade O6 = new Paygrade { Value = "O6", Description = "" };
        public static Paygrade O7 = new Paygrade { Value = "O7", Description = "" };
        public static Paygrade O8 = new Paygrade { Value = "O8", Description = "" };
        public static Paygrade O9 = new Paygrade { Value = "O9", Description = "" };
        public static Paygrade O10 = new Paygrade { Value = "O10", Description = "" };
        public static Paygrade GG1 = new Paygrade { Value = "GG1", Description = "" };
        public static Paygrade GG2 = new Paygrade { Value = "GG2", Description = "" };
        public static Paygrade GG3 = new Paygrade { Value = "GG3", Description = "" };
        public static Paygrade GG4 = new Paygrade { Value = "GG4", Description = "" };
        public static Paygrade GG5 = new Paygrade { Value = "GG5", Description = "" };
        public static Paygrade GG6 = new Paygrade { Value = "GG6", Description = "" };
        public static Paygrade GG7 = new Paygrade { Value = "GG7", Description = "" };
        public static Paygrade GG8 = new Paygrade { Value = "GG8", Description = "" };
        public static Paygrade GG9 = new Paygrade { Value = "GG9", Description = "" };
        public static Paygrade GG10 = new Paygrade { Value = "GG10", Description = "" };
        public static Paygrade GG11 = new Paygrade { Value = "GG11", Description = "" };
        public static Paygrade GG12 = new Paygrade { Value = "GG12", Description = "" };
        public static Paygrade GG13 = new Paygrade { Value = "GG13", Description = "" };
        public static Paygrade GG14 = new Paygrade { Value = "GG14", Description = "" };
        public static Paygrade GG15 = new Paygrade { Value = "GG15", Description = "" };
        public static Paygrade CON = new Paygrade { Value = "CON", Description = "" };

    }
}
