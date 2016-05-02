using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using AtwoodUtils;
using NHibernate;

namespace CommandCentral.ClientAccess.Service
{
    /// <summary>
    /// Describes the service and its implementation of the endpoints.
    /// </summary>
    [ServiceBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
    public class CommandCentralService : ICommandCentralService
    {

        #region Endpoints


        /// <summary>
        /// Allows for dynamic invocation of endpoints by using the EndpointsDescription dictionary to whitelist the endpoints.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<string> InvokeGenericEndpointAsync(Stream data, string endpoint)
        {

            return "";
            //We set these variables outside the try loop so that we can use them in the catch block if needed.
            Guid messageID = Guid.NewGuid();
            Guid clientID;
            MessageToken token = new MessageToken();

            //Add the CORS headers to the request.
            Utilities.AddCORSHeadersToResponse(WebOperationContext.Current);

            try
            {

                //Tell the host that we've received a message.
                Communicator.PostMessageToHost(string.Format("{0} invoked '{1}' @ {2}", messageID, endpoint, DateTime.Now), Communicator.MessagePriority.Informational);

                //Does the endpoint the client tried to invoke exist?
                EndpointDescription desc;
                if (!Endpoints.TryGetValue(endpoint, out desc))
                    throw new ServiceException(string.Format("The endpoint '{0}' is not valid.  If you're certain this should be an endpoint and you've checked your spelling, yell at Atwood.  For further issues, please call Atwood at 505-401-7252.", endpoint), ErrorTypes.Validation, HTTPStatusCodes.Not_Found);

                //Is the endpoint active?
                if (!desc.IsActive)
                    throw new ServiceException(string.Format("The endpoint '{0}' has been disabled.  Please yell at Atwood for more information.", endpoint), ErrorTypes.Validation, HTTPStatusCodes.Service_Unavailable);

                //Ok, this is going to be an actual request.  At this point we can ask for communication session from the session factory.
                var communicationSession = DataAccess.NHibernateHelper.CreateSession();

                //Process the message token.
                token = ProcessMessage(data, endpoint, messageID, communicationSession);

                //If this endpoint requires authentication, then get the session
                if (desc.RequiresAuthentication)
                {
                    token = AuthenticateMessage(token);
                    clientID = token.AuthenticationSession.Person.ID;

                    //Because the session was successfully authenticated, let's go ahead and update it, since this is now the most recent time it was used, regardless if anything fails after this,
                    //at least the client tried to use the session.
                    token.AuthenticationSession.LastUsedTime = DateTime.Now;
                    using (var transaction = token.CommunicationSession.BeginTransaction())
                    {
                        try
                        {
                            token.CommunicationSession.Update(token.AuthenticationSession);

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                /*

                //If this message allows logging, then log it.
                await token.DBInsert(true, desc.AllowArgumentLogging, desc.AllowResponseLogging);

                //Tell the host we've finished processing the message.
                Communicator.PostMessageToHost(string.Format("{0} processed @ {1}", messageID, DateTime.Now), Communicator.MessagePriority.Informational);

                //Invoke the endpoint's data method.
                //token = await desc.DataMethodAsync(token);

                //Build what will be our final return string.  We do this prior to updating the message as "handled" in case serializing the return container takes an appreciable about of time so that we don't show a false handled time.
                string finalResult = new Framework.ReturnContainer()
                {
                    ErrorMessage = null,
                    HasError = false,
                    ReturnValue = token.Result
                }.Serialize();

                //Update the token in the database, indicating that we have successfully handled it.
                token.HandledTime = DateTime.Now;
                token.State = MessageTokens.MessageStates.Handled;
                await token.DBUpdate(true, desc.AllowArgumentLogging, desc.AllowResponseLogging);

                //Tell the host we finished with the message.
                Communicator.PostMessageToHost(string.Format("{0} handled @ {1}", messageID, DateTime.Now), Communicator.MessagePriority.Informational);

                //Return our result.*/
                return "";

            }
            catch (ServiceException e) //If this exception is received, then it was an expected exception that we explicitly threw.  These kinds of exceptions are ok, and are used as validation errors, authorization errors, or authentication errors that caused the message not to be processable. (yes it's a word, fuck you)
            {
                /*//Update the message as an error.  If the message didn't get to the point that it was inserted into the database then it just won't update anything.
                MessageTokens.UpdateOnExpectedError(messageID, e.Message);

                //Tell the client we handled the reqest as a validation error.
                Communicator.PostMessageToHost(string.Format("{0} handled (validation error) @ {1} - '{2}'", messageID, DateTime.Now, e.Message), Communicator.MessagePriority.Warning);

                //Whatever the error was, put it in a return container and return that.
                return new Framework.ReturnContainer()
                {
                    ErrorMessage = e.Message,
                    HasError = true,
                    ErrorType = e.ErrorType,
                    ReturnValue = null
                }.Serialize();*/
            }
            catch (Exception e) //Any other exception is really bad.  It means something was thrown by the service or the CLR itself.  Catch this exception and alert the developers.
            {
                /*//Update the message as an error.  If the message didn't get to the point that it was inserted into the database then it just won't update anything.
                MessageTokens.UpdateOnFatalError(messageID, e.Message);

                //Log the error in the errors table.
                new Errors.Error()
                {
                    ID = Guid.NewGuid().ToString(),
                    InnerException = (e.InnerException == null) ? "" : e.InnerException.Message,
                    IsHandled = false,
                    LoggedInUserID = clientID,
                    Message = e.Message,
                    StackTrace = e.StackTrace,
                    Time = DateTime.Now
                }.DBInsert().Wait();

                //Tell the client we failed a request.
                Communicator.PostMessageToHost(string.Format("{0} failed @ {1} - '{2}'", messageID, DateTime.Now, e.Message), Communicator.MessagePriority.Critical);

                //Send an error email, informing everyone that we failed a request.
                UnifiedEmailHelper.SendFatalErrorEmail(token, e).Wait();

                //Return an error message to the client.  The details of the message were sent to the developers, but the details don't need to be sent to the client.
                return new Framework.ReturnContainer()
                {
                    ErrorMessage = "Well... this is awkward.  The CommandDB suffered a serious, fatal error.  We are extremely sorry about this.  \n\n\tAn email, text message, and smoke signals have been sent to the developers with the details of this error.  \n\n\tYou'll most likely be contacted shortly if we need further information about this crash such as what you were doing in the application and the order in which you did it.  Any help you can provide us will be appreciated.",
                    HasError = true,
                    ErrorType = ErrorTypes.Fatal,
                    ReturnValue = null
                }.Serialize();*/
            }


        }

        /// <summary>
        /// Returns documentation for a given endpoint in the form of a web page.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<Stream> GetDocumentationForEndpoint(string endpoint)
        {
            try
            {

                //Add the CORS headers to this request.
                Utilities.AddCORSHeadersToResponse(WebOperationContext.Current);

                //Set the outgoing type as HTML
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

                string result = null;

                //Is endpoint not null or empty?
                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    result = "The endpoint name can not be null or empty.";
                }
                else
                {

                    //Alright, before we do anything, try to get the endpoint.  If this endpoint doesn't exist, then we can just stop here.
                    EndpointDescription endpointDescription;
                    if (!Endpoints.TryGetValue(endpoint, out endpointDescription))
                    {
                        result = string.Format("The endpoint, '{0}', was not valid.  You can not request its documentation.  Try checking your spelling.", endpoint);
                    }
                    else
                    {
                        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                        string templateName = "UnifiedServiceFramework.Resources.Documentation.DocumentationTemplate.html";

                        string templatePage = null;
                        using (Stream stream = assembly.GetManifestResourceStream(templateName))
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            templatePage = await reader.ReadToEndAsync();
                        }

                        string requiredParamaters = "None";
                        if (endpointDescription.RequiredParameters != null && endpointDescription.RequiredParameters.Any())
                            requiredParamaters = string.Join(Environment.NewLine, endpointDescription.RequiredParameters.Select(x => string.Format("{0} <br />", x)));

                        string optionalParameters = "None";
                        if (endpointDescription.OptionalParameters != null && endpointDescription.OptionalParameters.Any())
                            optionalParameters = string.Join(Environment.NewLine, endpointDescription.OptionalParameters.Select(x => string.Format("{0} <br />", x)));

                        string requiredSpecialPerms = "None";
                        if (endpointDescription.RequiredSpecialPermissions != null && endpointDescription.RequiredSpecialPermissions.Any())
                            requiredSpecialPerms = string.Join(",", endpointDescription.RequiredSpecialPermissions);

                        string authNote = "None";
                        if (!string.IsNullOrWhiteSpace(endpointDescription.AuthorizationNote))
                            authNote = endpointDescription.AuthorizationNote;

                        string output = "Ask Atwood to write this";
                        if (endpointDescription.ExampleOutput != null)
                            output = endpointDescription.ExampleOutput();

                        result = string.Format(templatePage, endpoint, endpointDescription.Description, requiredSpecialPerms, requiredParamaters, optionalParameters,
                            authNote, endpointDescription.RequiresAuthentication.ToString(), endpointDescription.AllowArgumentLogging.ToString(), endpointDescription.AllowResponseLogging.ToString(),
                            endpointDescription.IsActive.ToString(), output);
                    }

                }

                byte[] resultBytes = Encoding.UTF8.GetBytes(result);
                return new MemoryStream(resultBytes);

            }
            catch
            {

                throw;
            }
        }

        /// <summary>
        /// Returns documentation for all endpoints.
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetAllDocumentation()
        {
            try
            {
                //Add the CORS headers to this request.
                Utilities.AddCORSHeadersToResponse(WebOperationContext.Current);

                //Set the outgoing type as HTML
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string templateName = "UnifiedServiceFramework.Resources.Documentation.AllEndpointsTemplate.html";

                string templatePage = null;
                using (Stream stream = assembly.GetManifestResourceStream(templateName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    templatePage = await reader.ReadToEndAsync();
                }

                string endpointTemplate = @"<li class='list-group-item'><a href='./man/{0}'>{0}</a></li>";

                string endpoints = "";

                Endpoints.ToList().OrderBy(x => x.Key).ToList().ForEach(x =>
                {
                    endpoints += string.Format(endpointTemplate, x.Key) + Environment.NewLine;
                });

                byte[] resultBytes = Encoding.UTF8.GetBytes(string.Format(templatePage, endpoints));
                return new MemoryStream(resultBytes);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Processes a message token, turning the data stream from the POST data layer into a dictionary.  This method also authenticates the API Key.
        /// </summary>
        /// <param name="data">The data stream from the POST data parameter.</param>
        /// <param name="endpoint">The endpoint that was invoked.</param>
        /// <param name="messageID">The ID of the message.</param>
        /// <param name="communicationSession">The NHibernate session that will be used throughout the lifetime of this request.</param>
        /// <returns></returns>
        private static MessageToken ProcessMessage(Stream data, string endpoint, Guid messageID, ISession communicationSession)
        {
            try
            {
                //Convert the stream into the parameters
                Dictionary<string, object> args = Utilities.ConvertPostDataToDict(data);

                //Get and authenticate the APIkey
                if (!args.ContainsKey("apikey"))
                    throw new ServiceException("You must send an API Key.", ErrorTypes.Validation, HTTPStatusCodes.Bad_Request);
                string apiKey = args["apikey"] as string;

                //Try to get the API key.
                APIKey key;
                
                using (var transaction = communicationSession.BeginTransaction())
                {
                    try
                    {
                        key = communicationSession.Get<APIKey>(apiKey);

                        if (key == null)
                            throw new ServiceException("That API Key is invalid.", ErrorTypes.Validation, HTTPStatusCodes.Forbiden);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                //Build the message token and return it.
                return new MessageToken
                {
                    APIKey = key,
                    Args = args,
                    CallTime = DateTime.Now,
                    Endpoint = endpoint,
                    ID = messageID,
                    State = MessageStates.Active,
                    CommunicationSession = communicationSession
                };
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Authenticates the message by using the authentication token from the args list to retrieve the client's session.  
        /// <para />
        /// This method also checks the client's permissions and sets them on the session parameter of the message token for later use.
        /// <para />
        /// This method also ensures that the session has not become inactive
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static MessageToken AuthenticateMessage(MessageToken token)
        {
            try
            {
                //Get the session for this authentication token.
                if (!token.Args.ContainsKey("authenticationtoken"))
                    throw new ServiceException("You must send an authentication token.", ErrorTypes.Validation, HTTPStatusCodes.Bad_Request);
                string authenticationToken = token.Args["authenticationtoken"] as string;

                //Alright let's try to get the session.
                using (var transaction = token.CommunicationSession.BeginTransaction())
                {
                    try
                    {
                        var authenticationSession = token.CommunicationSession.Get<AuthenticationSession>(authenticationToken);

                        if (authenticationSession == null)
                            throw new ServiceException("That authentication token does not belong to an actual authenticated session.  Consider logging in so as to attain a token.", ErrorTypes.Authentication, HTTPStatusCodes.Bad_Request);

                        //Ok so we got a session, is it valid?
                        if (authenticationSession.IsExpired())
                            throw new ServiceException("The session has timed out.  Please sign back in.", ErrorTypes.Authentication, HTTPStatusCodes.Unauthorized);

                        //Since the session is in fact valid, we can go ahead and tack it onto the token.
                        token.AuthenticationSession = authenticationSession;

                        transaction.Commit();
                    }
                    catch
                    {
                        if (!transaction.WasCommitted)
                            transaction.Rollback();
                        throw;
                    }
                }

                return token;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region ServiceManagement

        /// <summary>
        /// The list of all endpoints that are exposed to the client.
        /// </summary>
        public static ConcurrentDictionary<string, EndpointDescription> Endpoints = new ConcurrentDictionary<string, EndpointDescription>();

        #endregion


    }
}
