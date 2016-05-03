using System;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Provides a custom exception meant to be used to differentiate between this and normal CLR exceptions.
    /// </summary>
    public class ServiceException : Exception
    {

        private ErrorTypes _errorType = ErrorTypes.Null;
        /// <summary>
        /// Indicates what type of error is contained in this exception.
        /// </summary>
        public ErrorTypes ErrorType
        {
            get
            {
                return _errorType;
            }
            set
            {
                _errorType = value;
            }
        }

        /// <summary>
        /// The HTTP Status Code associated with this error.
        /// </summary>
        public HttpStatusCodes HttpStatusCode { get; set; }

        /// <summary>
        /// Creates a new instance of a ServiceException
        /// </summary>
        public ServiceException(ErrorTypes errorType, HttpStatusCodes httpStatusCode)
        {
            ErrorType = errorType;
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Creates a new instance of a ServiceException with the given message
        /// </summary>
        public ServiceException(string message, ErrorTypes errorType, HttpStatusCodes httpStatusCode)
            : base(message)
        {
            ErrorType = errorType;
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Creates a new instance of a ServiceException with the given message and inner exception.
        /// </summary>
        public ServiceException(string message, Exception inner, ErrorTypes errorType, HttpStatusCodes httpStatusCode)
            : base(message, inner)
        {
            ErrorType = errorType;
            HttpStatusCode = httpStatusCode;
        }
    }
}
