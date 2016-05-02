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
using AtwoodUtils;

namespace UnifiedServiceFramework.Framework
{
    /// <summary>
    /// Describes a single error.
    /// </summary>
    public class Error
    {
        #region Properties

        /// <summary>
        /// The unique ID assigned to this error
        /// </summary>
        public virtual Guid ID { get; set; }

        /// <summary>
        /// The message that was raised for this error
        /// </summary>
        public virtual string Message { get; set; }

        /// <summary>
        /// The stack trace that lead to this error
        /// </summary>
        public virtual string StackTrace { get; set; }

        /// <summary>
        /// Any inner exception's message
        /// </summary>
        public virtual string InnerException { get; set; }

        /// <summary>
        /// The ID of the session's user when this error occurred.
        /// </summary>
        public virtual string LoggedInUserID { get; set; }

        /// <summary>
        /// The Date/Time this error occurred.
        /// </summary>
        public virtual DateTime Time { get; set; }

        /// <summary>
        /// Indicates whether or not the development/maintenance team has dealt with what caused this error.
        /// </summary>
        public virtual bool IsHandled { get; set; }

        #endregion
    }
}
