using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public static class AuthorizationRules
    {
        public static NeverRule Never = new NeverRule();
        public static IfSelfRule IfSelf = new IfSelfRule();
        public static IfInChainOfCommandRule IfInChainOfCommand = new IfInChainOfCommandRule();

        public class NeverRule : IAuthorizationRule
        {
            public bool AuthorizationOperation(AuthorizationToken authToken)
            {
                return false;
            }
        }

        public class IfSelfRule : IAuthorizationRule
        {
            public bool AuthorizationOperation(AuthorizationToken authToken)
            {
                return authToken.Client.Id == authToken.EditedPerson.Id;
            }
        }

        public class IfInChainOfCommandRule : IAuthorizationRule
        {
            public bool AuthorizationOperation(AuthorizationToken authToken)
            {
                return authToken.Client.IsInChainOfCommandOf(authToken.EditedPerson);
            }
        }
    }
}
