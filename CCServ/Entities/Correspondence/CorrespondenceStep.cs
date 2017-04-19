using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using CCServ.Entities.ReferenceLists;
using CCServ.ClientAccess;
using System.Globalization;
using AtwoodUtils;

namespace CCServ.Entities.Correspondence
{
    /// <summary>
    /// Describes a single correspondence step which is used in the parents object as a list.
    /// </summary>
    public class CorrespondenceStep
    {

        #region Properties

        /// <summary>
        /// The unique Id for this step.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The correspondence that owns this step.
        /// </summary>
        [ConditionalJsonIgnore]
        public virtual Correspondence Correspondence { get; set; }

        /// <summary>
        /// The number order of this step.  If this step shares its order when another step, then they occur at the same time.
        /// </summary>
        public virtual int Order { get; set; }

        #endregion

    }
}
