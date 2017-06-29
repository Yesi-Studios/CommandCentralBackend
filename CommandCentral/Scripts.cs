using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Entities.ReferenceLists.Watchbill;
using CommandCentral.Entities;
using System.Diagnostics;
using NHibernate.Criterion;
using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;

namespace CommandCentral
{
    static class Scripts
    {

        //[ServiceManagement.StartMethod(Priority = 1)]
        private static void WatchbillStats(CLI.Options.LaunchOptions launchOptions)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                var watchbill = session.QueryOver<Watchbill>().List().First();

                var data = watchbill.WatchShifts.Select(x => x.WatchAssignment).GroupBy(x => x.PersonAssigned.Department);

                string text = "";
                foreach (var group in data)
                {

                    int totalDep = session.QueryOver<Person>().Where(x => x.Department.Id == group.Key.Id).RowCount();
                    int total = session.QueryOver<Person>().RowCount();

                    text += "{0} : {1}% ({2}/{3}) vs {4}% ({5}/{6})"
                        .With(group.Key,
                        Math.Round(((double)group.ToList().Count / (double)watchbill.WatchShifts.Select(x => x.WatchAssignment).Count()) * 100, 2),
                        group.ToList().Count,
                        watchbill.WatchShifts.Select(x => x.WatchAssignment).Count(),
                        Math.Round(((double)totalDep / (double)total) * 100, 2),
                        totalDep,
                        total);
                    text += Environment.NewLine;
                }
            }
        }
        
    }
}
