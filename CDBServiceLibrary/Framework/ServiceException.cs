using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedServiceFramework.Framework
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
        /// Creates a new instance of a ServiceException
        /// </summary>
        public ServiceException(ErrorTypes errorType)
        {
            this.ErrorType = errorType;
        }

        /// <summary>
        /// Creates a new instance of a ServiceException with the given message
        /// </summary>
        public ServiceException(string message, ErrorTypes errorType)
            : base(message)
        {
            this.ErrorType = errorType;
        }

        /// <summary>
        /// Creates a new instance of a ServiceException with the given message and inner exception.
        /// </summary>
        public ServiceException(string message, Exception inner, ErrorTypes errorType)
            : base(message, inner)
        {
            this.ErrorType = errorType;
        }
    }
}
