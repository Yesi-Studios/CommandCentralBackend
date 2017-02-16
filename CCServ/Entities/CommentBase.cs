using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities
{
    /// <summary>
    /// The base class for a comment.  Should be implemented by any object that intends to be a "comment" section.
    /// </summary>
    public abstract class CommentBase
    {
        #region Properties

        /// <summary>
        /// The unique Id of this comment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The text of this comment.
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// The time this comment was made.
        /// </summary>
        public virtual DateTime DateCreated { get; set; }

        /// <summary>
        /// The creator of this comment.
        /// </summary>
        public virtual Person Creator { get; set; }

        #endregion
    }
}
