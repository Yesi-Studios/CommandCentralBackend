using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Defines a single muster report which is the report that goes our when muster has been finalized for a given day.
    /// <para/>
    /// We could build this information every time someone asked for it by just querying back over the database but I figure storage is cheaper than CPU time so I may as well store the reports after we make them.
    /// </summary>
    public class MusterReport
    {
        public virtual Guid Id { get; set; }

        public virtual int MusterDayOfYear { get; set; }

        public virtual int MusterYear { get; set; }



    }
}
