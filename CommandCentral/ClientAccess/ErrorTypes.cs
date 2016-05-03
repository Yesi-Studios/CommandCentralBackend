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
        /// Indicates that the client attempted to take a lock on a profile for which the client is not allowed to take a lock.
        /// </summary>
        LockImpossible,
        /// <summary>
        /// Indicates a fatal occurred due to an error in Command Central's code.
        /// </summary>
        Fatal,
        /// <summary>
        /// Indicates that no error occurred or an error has not yet been set.
        /// </summary>
        Null
    }
}
