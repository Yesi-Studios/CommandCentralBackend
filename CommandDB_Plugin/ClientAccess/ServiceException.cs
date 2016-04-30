using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Provides a custom exception meant to be used to differentiate between this and normal CLR exceptions.
    /// </summary>
    public class ServiceException : Exception
    {

        private ErrorTypes _errorType = ErrorTypes.NULL;
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
        public HTTPStatusCodes HTTPStatusCode { get; set; }

        /// <summary>
        /// Creates a new instance of a ServiceException
        /// </summary>
        public ServiceException(ErrorTypes errorType, HTTPStatusCodes httpStatusCode)
        {
            this.ErrorType = errorType;
            this.HTTPStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Creates a new instance of a ServiceException with the given message
        /// </summary>
        public ServiceException(string message, ErrorTypes errorType, HTTPStatusCodes httpStatusCode)
            : base(message)
        {
            this.ErrorType = errorType;
            this.HTTPStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Creates a new instance of a ServiceException with the given message and inner exception.
        /// </summary>
        public ServiceException(string message, Exception inner, ErrorTypes errorType, HTTPStatusCodes httpStatusCode)
            : base(message, inner)
        {
            this.ErrorType = errorType;
            this.HTTPStatusCode = httpStatusCode;
        }
    }
}
