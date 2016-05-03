using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.DataAccess;
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

        #region ServiceManagement

        /// <summary>
        /// The list of all endpoints that are exposed to the client.
        /// </summary>
        public static readonly ConcurrentDictionary<string, EndpointDescription> Endpoints = new ConcurrentDictionary<string, EndpointDescription>();

        /// <summary>
        /// Static constructor that builds the endpoints.  This is how we register new endpoints.
        /// </summary>
        static CommandCentralService()
        {
            Entities.Person.EndpointDescriptions.ToList().ForEach(x => Endpoints.AddOrUpdate(x.Key, x.Value,
                (key, value) =>
                {
                    throw new Exception();
                }));
        }

        #endregion


        /// <summary>
        /// Allows for dynamic invocation of endpoints by using the EndpointsDescription dictionary to whitelist the endpoints.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public string InvokeGenericEndpointAsync(Stream data, string endpoint)
        {

            //We set these variables outside the try loop so that we can use them in the catch block if needed.
            Guid messageId = Guid.NewGuid();
            MessageToken token;

            //Add the CORS headers to the request.
            Utilities.AddCorsHeadersToResponse(WebOperationContext.Current);

            try
            {

                //Tell the host that we've received a message.
                Communicator.PostMessageToHost(string.Format("{0} invoked '{1}' @ {2}", messageId, endpoint, DateTime.Now), Communicator.MessagePriority.Informational);

                //Does the endpoint the client tried to invoke exist?
                EndpointDescription desc;
                if (!Endpoints.TryGetValue(endpoint, out desc))
                    throw new ServiceException(string.Format("The endpoint '{0}' is not valid.  If you're certain this should be an endpoint and you've checked your spelling, yell at Atwood.  For further issues, please call Atwood at 505-401-7252.", endpoint), ErrorTypes.Validation, HttpStatusCodes.NotFound);

                //Is the endpoint active?
                if (!desc.IsActive)
                    throw new ServiceException(string.Format("The endpoint '{0}' has been disabled.  Please yell at Atwood for more information.", endpoint), ErrorTypes.Validation, HttpStatusCodes.ServiceUnavailable);

                //Ok, this is going to be an actual request.  At this point we can ask for communication session from the session factory.
                var communicationSession = NHibernateHelper.CreateSession();

                //Process the message token.
                token = ProcessMessage(data, endpoint, messageId, communicationSession);

                //If this endpoint requires authentication, then get the session
                if (desc.RequiresAuthentication)
                {
                    token = AuthenticateMessage(token);

                    //Because the session was successfully authenticated, let's go ahead and update it, since this is now the most recent time it was used, regardless if anything fails after this,
                    //at least the client tried to use the session.
                    token.AuthenticationSession.LastUsedTime = DateTime.Now;
                    using (var transaction = token.CommunicationSession.BeginTransaction())
                    {
                        try
                        {
                            token.CommunicationSession.SaveOrUpdate(token.AuthenticationSession);

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                //Now that we're done building the token, let's also save/update that.
                using (var transaction = token.CommunicationSession.BeginTransaction())
                {
                    try
                    {
                        token.CommunicationSession.SaveOrUpdate(token);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                //Tell the host we've finished processing the message.
                Communicator.PostMessageToHost(string.Format("{0} processed @ {1}", token.Id, DateTime.Now), Communicator.MessagePriority.Informational);

                //Invoke the endpoint's data method.
                token = desc.DataMethod(token);

                //Build what will be our final return string.  We do this prior to updating the message as "handled" in case serializing the return container takes an appreciable about of time so that we don't show a false handled time.
                string finalResult = new ReturnContainer
                {
                    ErrorMessages = new List<string>(),
                    HasError = false,
                    ReturnValue = token.Result,
                    ErrorType = ErrorTypes.Null,
                    StatusCode = HttpStatusCodes.Ok
                }.Serialize();

                //Update the token in the database, indicating that we have successfully handled it.
                token.HandledTime = DateTime.Now;
                token.State = MessageStates.Handled;

                //Update the token.
                using (var transaction = token.CommunicationSession.BeginTransaction())
                {
                    try
                    {
                        token.CommunicationSession.SaveOrUpdate(token);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                //Tell the host we finished with the message.
                Communicator.PostMessageToHost(string.Format("{0} handled @ {1}", token.Id, DateTime.Now), Communicator.MessagePriority.Informational);

                //Return our result.
                return finalResult;

            }
            catch (ServiceException e) //If this exception is received, then it was an expected exception that we explicitly threw.  These kinds of exceptions are ok, and are used as validation errors, authorization errors, or authentication errors that caused the message not to be processable. (yes it's a word, fuck you)
            {
                return "Fuck";
            }
            catch (Exception e) //Any other exception is really bad.  It means something was thrown by the service or the CLR itself.  Catch this exception and alert the developers.
            {
                return "Fuck";
            }


        }

        /// <summary>
        /// Returns documentation for a given endpoint in the form of a web page.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<Stream> GetDocumentationForEndpoint(string endpoint)
        {
            //Add the CORS headers to this request.
            Utilities.AddCorsHeadersToResponse(WebOperationContext.Current);

            //Set the outgoing type as HTML
            Debug.Assert(WebOperationContext.Current != null, "WebOperationContext.Current != null");
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            string result;

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
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string templateName = "UnifiedServiceFramework.Resources.Documentation.DocumentationTemplate.html";

                    string templatePage;
                    using (Stream stream = assembly.GetManifestResourceStream(templateName))
                    {
                        Debug.Assert(stream != null, "stream != null");
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            templatePage = await reader.ReadToEndAsync();
                        }
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
                        authNote, endpointDescription.RequiresAuthentication, endpointDescription.AllowArgumentLogging, endpointDescription.AllowResponseLogging,
                        endpointDescription.IsActive, output);
                }

            }

            byte[] resultBytes = Encoding.UTF8.GetBytes(result);
            return new MemoryStream(resultBytes);
        }

        /// <summary>
        /// Returns documentation for all endpoints.
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetAllDocumentation()
        {
            //Add the CORS headers to this request.
            Utilities.AddCorsHeadersToResponse(WebOperationContext.Current);

            //Set the outgoing type as HTML
            Debug.Assert(WebOperationContext.Current != null, "WebOperationContext.Current != null");
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            Assembly assembly = Assembly.GetExecutingAssembly();
            string templateName = "UnifiedServiceFramework.Resources.Documentation.AllEndpointsTemplate.html";

            string templatePage;
            using (Stream stream = assembly.GetManifestResourceStream(templateName))
            {
                Debug.Assert(stream != null, "stream != null");
                using (StreamReader reader = new StreamReader(stream))
                {
                    templatePage = await reader.ReadToEndAsync();
                }
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Processes a message token, turning the data stream from the POST data layer into a dictionary.  This method also authenticates the API Key.
        /// </summary>
        /// <param name="data">The data stream from the POST data parameter.</param>
        /// <param name="endpoint">The endpoint that was invoked.</param>
        /// <param name="messageId">The Id of the message.</param>
        /// <param name="communicationSession">The NHibernate session that will be used throughout the lifetime of this request.</param>
        /// <returns></returns>
        private static MessageToken ProcessMessage(Stream data, string endpoint, Guid messageId, ISession communicationSession)
        {
            //Convert the stream into the parameters
            Dictionary<string, object> args = Utilities.ConvertPostDataToDict(data);

            //Get and authenticate the APIkey
            if (!args.ContainsKey("apikey"))
                throw new ServiceException("You must send an API Key.", ErrorTypes.Validation, HttpStatusCodes.BadRequest);
            string apiKey = args["apikey"] as string;

            //Try to get the API key.
            ApiKey key;
                
            using (var transaction = communicationSession.BeginTransaction())
            {
                try
                {
                    key = communicationSession.Get<ApiKey>(apiKey);

                    if (key == null)
                        throw new ServiceException("That API Key is invalid.", ErrorTypes.Validation, HttpStatusCodes.Forbiden);

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
                ApiKey = key,
                Args = args,
                CallTime = DateTime.Now,
                Endpoint = endpoint,
                Id = messageId,
                State = MessageStates.Active,
                CommunicationSession = communicationSession
            };
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
            //Get the session for this authentication token.
            if (!token.Args.ContainsKey("authenticationtoken"))
                throw new ServiceException("You must send an authentication token.", ErrorTypes.Validation, HttpStatusCodes.BadRequest);
            string authenticationToken = token.Args["authenticationtoken"] as string;

            //Alright let's try to get the session.
            using (var transaction = token.CommunicationSession.BeginTransaction())
            {
                try
                {
                    var authenticationSession = token.CommunicationSession.Get<AuthenticationSession>(authenticationToken);

                    if (authenticationSession == null)
                        throw new ServiceException("That authentication token does not belong to an actual authenticated session.  Consider logging in so as to attain a token.", ErrorTypes.Authentication, HttpStatusCodes.BadRequest);

                    //Ok so we got a session, is it valid?
                    if (authenticationSession.IsExpired())
                        throw new ServiceException("The session has timed out.  Please sign back in.", ErrorTypes.Authentication, HttpStatusCodes.Unauthorized);

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

        #endregion

    }
}
