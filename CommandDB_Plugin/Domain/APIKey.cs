using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Domain
{
    public class APIKey : DataAccess.CachedModel<APIKey>
    {

        #region Properties

        public virtual string ID { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if a given api key is valid.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static bool IsAPIKeyValid(object apiKey)
        {
            return APIKey.GetOne(apiKey) == null;
        }

        #endregion

    }
}
