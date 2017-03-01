using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities
{
    /// <summary>
    /// A comment.  It's assigned to an object where users can comment on it.
    /// </summary>
    public class Comment
    {

        #region Properties

        /// <summary>
        /// The unique Id of this comment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The person who created this comment.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// This is the text of the comment.
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// The datetime at which this comment was made.
        /// </summary>
        public virtual DateTime Time { get; set; }

        /// <summary>
        /// The entity for which this comment was made.
        /// </summary>
        public virtual object Entity { get; set; }

        #endregion

    }
}
