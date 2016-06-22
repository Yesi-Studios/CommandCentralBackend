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

        public class NeverRule : IAuthorizationRule
        {
            public bool AuthorizationOperation(AuthorizationToken authToken)
            {
                return false;
            }
        }
    }
}
