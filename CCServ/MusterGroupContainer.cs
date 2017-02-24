using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ
{
    public class MusterGroupContainer
    {
        public string GroupTitle { get; set; }
        public int Total { get; set; }
        public int Mustered { get; set; }
        public double Percentage
        {
            get
            {
                return Math.Round(((double)Mustered / (double)Total) * 100, 2);
            }
        }

        public override string ToString()
        {
            return "{0} : {1}% ({2}/{3})".FormatS(GroupTitle, Percentage, Mustered, Total);
        }
    }
}
