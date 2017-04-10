using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ
{
    /// <summary>
    /// The interface that makes an object commentable.
    /// </summary>
    public interface ICommentable
    {
        /// <summary>
        /// The comments.
        /// </summary>
        IList<Entities.Comment> Comments { get; set; }
    }
}
