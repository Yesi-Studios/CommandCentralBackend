using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Rules
{
    /// <summary>
    /// Returns true if the client's Id matches the person's Id.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IfSelfRule : AuthorizationRuleBase
    {
        /// <summary>
        /// Returns true if the client's Id matches the person's Id.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            //If the client person is null, then return false.
            if (authToken.PersonFromClient == null)
                return false;

            return authToken.Client.Id == authToken.PersonFromClient.Id;
        }
    }
}
