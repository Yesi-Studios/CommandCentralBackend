using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CCServ.Entities;
using System.Collections;
using AtwoodUtils;
using Newtonsoft.Json;

namespace CCServ.Entities
{
    /// <summary>
    /// Describes a single file attachment.
    /// </summary>
    public class FileAttachment
    {
        public virtual Guid Id { get; set; }

        [JsonIgnore]
        public virtual string FilePath { get; set; }
    }
}
