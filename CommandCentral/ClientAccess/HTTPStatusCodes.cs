namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// List of the most common HTTP status codes.
    /// </summary>
    public enum HttpStatusCodes
    {
        /// <summary>
        /// The request has succeeded. The information returned with the response is dependent on the method used in the request.
        /// </summary>
        Ok = 200,
        /// <summary>
        /// The requested resource has different choices and cannot be resolved into one. For example, there may be several index.html pages depending on which language is wanted (such as Dutch).
        /// </summary>
        MultipleChoices = 300,
        /// <summary>
        /// The requested resource has been assigned a new permanent URI and any future references to this resource should use one of the returned URIs.
        /// </summary>
        MovedPermanently = 301,
        /// <summary>
        /// The requested resource resides temporarily under a different URI. Since the redirection might be altered on occasion, the client SHOULD continue to use the Request-URI for future requests.
        /// </summary>
        Found = 302,
        /// <summary>
        /// The requested resource resides temporarily under a different URI. Since the redirection MAY be altered on occasion, the client SHOULD continue to use the Request-URI for future requests. 
        /// This response is only cacheable if indicated by a Cache-Control or Expires header field.
        /// </summary>
        TemporaryRedirect = 307,
        /// <summary>
        /// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications.
        /// </summary>
        BadRequest = 400,
        /// <summary>
        /// The request requires user authentication. The response MUST include a WWW-Authenticate header field containing a challenge applicable to the requested resource.
        /// </summary>
        Unauthorized = 401,
        /// <summary>
        /// The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated.
        /// </summary>
        Forbiden = 403,
        /// <summary>
        /// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
        /// </summary>
        NotFound = 404,
        /// <summary>
        /// The server encountered an unexpected condition which prevented it from fulfilling the request.
        /// </summary>
        InternalServerError = 500,
        /// <summary>
        /// The server does not support the functionality required to fulfill the request. This is the appropriate response when the server does not recognize the request method and is not capable of supporting it for any resource.
        /// </summary>
        NotImplemented = 501,
        /// <summary>
        /// Your web server is unable to handle your HTTP request at the time.
        /// </summary>
        ServiceUnavailable = 503,
        /// <summary>
        /// The server is stating the account you have currently logged in as does not have permission to perform the action you are attempting. You may be trying to upload to the wrong directory or trying to delete a file.
        /// </summary>
        PermissionDenied = 550

    }
}
