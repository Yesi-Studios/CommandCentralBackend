using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;

namespace CCServ.Tests
{
    public static class TestEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Causes an exception to be thrown.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "TestException", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_TestException(MessageToken token)
        {
            #if DEBUG
                throw new Exception("TEST TEST TEST");
            #endif
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Causes a critical message.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "CriticalMessage", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_CriticalMessage(MessageToken token)
        {
            #if DEBUG
                Logging.Log.Critical("TEST TEST TEST");
            #endif
        }
    }
}
