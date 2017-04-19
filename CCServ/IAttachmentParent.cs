using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ
{
    /// <summary>
    /// The interface that makes an object have attachments.
    /// </summary>
    public interface IAttachmentParent
    {
        /// <summary>
        /// The attachments.
        /// </summary>
        IList<Entities.FileAttachment> Attachments { get; set; }
    }
}
