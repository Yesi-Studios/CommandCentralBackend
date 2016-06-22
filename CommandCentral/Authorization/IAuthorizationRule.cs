using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Represents a common interface for authorization rules.
    /// </summary>
    public interface IAuthorizationRule
    {
        /// <summary>
        /// The operation to be run when this rule is invoked.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        bool AuthorizationOperation(AuthorizationToken authToken);
    }
}
