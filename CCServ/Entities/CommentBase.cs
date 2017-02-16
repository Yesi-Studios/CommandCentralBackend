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
    public class CommentBase
    {
        #region Properties

        /// <summary>
        /// The unique Id of this comment.
        /// </summary>
        public Guid Id { get; set; }




        #endregion
    }
}
