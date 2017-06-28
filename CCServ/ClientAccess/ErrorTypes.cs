using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// A list of error types.
    /// </summary>
    public enum ErrorTypes
    {
        /// <summary>
        /// Indicates that an error occur while attempting to validate one or more parameters in the request. 
        /// </summary>
        Validation,
        /// <summary>
        /// Indicates that an error occurred due to the client not being allowed to perform a requested action.
        /// </summary>
        Authorization,
        /// <summary>
        /// Indicates that an error occurred during authentication (Eg. The authentication token points to a timed out session.)
        /// </summary>
        Authentication,
        /// <summary>
        /// Indicates that the client attempted to take a lock on a profile for which a lock already exists.
        /// </summary>
        LockOwned,
        /// <summary>
        /// Indicates a fatal occurred due to an error in Command Central's code.
        /// </summary>
        Fatal,
        /// <summary>
        /// Indicates that no error occurred or an error has not yet been set.
        /// </summary>
        Null
    }

    /// <summary>
    /// Extensions for dealing with the error types.
    /// </summary>
    public static class ErrorTypesExtensions
    {
        /// <summary>
        /// Returns the matching http standard error type.
        /// </summary>
        /// <param name="errorType"></param>
        /// <returns></returns>
        public static HttpStatusCode GetMatchStatusCode(this ErrorTypes errorType)
        {
            switch (errorType)
            {
                case ErrorTypes.Authentication:
                    return HttpStatusCode.Forbidden;
                case ErrorTypes.Authorization:
                    return HttpStatusCode.Forbidden;
                case ErrorTypes.Fatal:
                    return HttpStatusCode.InternalServerError;
                case ErrorTypes.LockOwned:
                    return HttpStatusCode.Forbidden;
                case ErrorTypes.Null:
                    return HttpStatusCode.OK;
                case ErrorTypes.Validation:
                    return HttpStatusCode.BadRequest;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
