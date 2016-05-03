using System;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single error.
    /// </summary>
    public class Error
    {
        #region Properties

        /// <summary>
        /// The unique Id assigned to this error
        /// </summary>
        public virtual Guid Id { get; set; }

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
        /// The Id of the session's user when this error occurred.
        /// </summary>
        public virtual string LoggedInUserId { get; set; }

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
