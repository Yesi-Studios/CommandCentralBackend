using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public class AuthorizationEvaluationResult
    {
        public List<string> FailedProperties { get; set; }

        public bool HasErrors
        {
            get
            {
                return FailedProperties.Any();
            }
        }

        public AuthorizationEvaluationResult()
        {
            FailedProperties = new List<string>();
        }

    }
}
