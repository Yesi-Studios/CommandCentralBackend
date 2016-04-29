using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Concurrent;
using System.Reflection;
using MySql.Data.MySqlClient;
using MySql.Data.Common;
using UnifiedServiceFramework.Framework;
using AtwoodUtils;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single billet along with its data access methods and other members.
    /// </summary>
    public class Billet : CachedModel<Billet>
    {

        #region Properties

        /// <summary>
        /// The unique ID assigned to this Billet
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The title of this billet.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The ID Number of this Billet.  This is also called the Billet ID Number or BIN.
        /// </summary>
        public string IDNumber { get; set; }

        /// <summary>
        /// The suffix code of this Billet.  This is also called the Billet Suffix Code or BSC.
        /// </summary>
        public string SuffixCode { get; set; }

        /// <summary>
        /// A free form text field intended to store notes/remarks about this billet.
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// The designation assigned to a Billet.  For an enlisted Billet, this is the Rate the Billet is intended for.  For officers, this is their designation.
        /// </summary>
        public string Designation { get; set; }

        /// <summary>
        /// The funding line that pays for this particular billet.
        /// </summary>
        public string Funding { get; set; }

        /// <summary>
        /// The NEC assigned to this billet.
        /// </summary>
        public NEC NEC { get; set; }

        /// <summary>
        /// The UIC assigned to this billet.
        /// </summary>
        public UIC UIC { get; set; }

        #endregion

        /// <summary>
        /// Maps a billet to the database.
        /// </summary>
        public class BilletMapping : ClassMap<Billet>
        {
            /// <summary>
            /// Maps a billet to the database.
            /// </summary>
            public BilletMapping()
            {
                Table("billets");

                Id(x => x.ID);

                Map(x => x.Title).Length(40).Not.Nullable();
                Map(x => x.IDNumber).Length(10).Not.Nullable().Unique();
                Map(x => x.SuffixCode).Length(10).Not.Nullable().Unique();
                Map(x => x.Remarks).Length(100).Nullable();
                Map(x => x.Designation).Length(10).Not.Nullable().Unique();
                Map(x => x.Funding).Length(25).Not.Nullable();
                References(x => x.NEC).Not.Nullable();
                References(x => x.UIC).Not.Nullable();
            }
        }

    }
}
