using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ
{
    /// <summary>
    /// Describes the different account history types.
    /// </summary>
    public enum AccountHistoryEventTypes
    {
        /// <summary>
        /// The initial creation event of the account.
        /// </summary>
        Creation,
        /// <summary>
        /// A login event.
        /// </summary>
        Login,
        /// <summary>
        /// A logout event that results in the invalidation of a session.
        /// </summary>
        Logout,
        /// <summary>
        /// A failed login event.
        /// </summary>
        FailedLogin,
        /// <summary>
        /// The registration process was successfully begun.
        /// </summary>
        RegistrationStarted,
        /// <summary>
        /// The registration process was completed.
        /// </summary>
        RegistrationCompleted,
        /// <summary>
        /// The password reset process was started.
        /// </summary>
        PasswordResetInitiated,
        /// <summary>
        /// The password of a person account was reset.
        /// </summary>
        PasswordResetCompleted
    }
}
