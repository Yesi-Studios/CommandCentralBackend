using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Config
{
    /// <summary>
    /// Contains string constants for parameter names expected from the client.
    /// </summary>
    public static class ParamNames
    {
        /// <summary>
        /// The string used for the 'apikey' parameter.
        /// </summary>
        public const string API_KEY = "apikey";

        /// <summary>
        /// The string used for the 'authenticationtoken' parameter.
        /// </summary>
        public const string AUTHENTICATION_TOKEN = "authenticationtoken";

        /// <summary>
        /// The string used for the person object's id.
        /// </summary>
        public const string PERSON_ID = "personid";
    }
}
