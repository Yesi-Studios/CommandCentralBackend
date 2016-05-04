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
            MessageToken token = new MessageToken 
            { 
                Id = Guid.NewGuid(), 
                CallTime = DateTime.Now, 
                Endpoint = endpoint, 
                State = MessageStates.Received
            };

            //Tell the client we have a request.
            Communicator.PostMessageToHost("{0} | {1} | {2}\n\t\tCall Time: {3}".FormatS(token.Id.ToString(), token.Endpoint, token.State.ToString(), token.CallTime.ToString()), Communicator.MessagePriority.Informational);

            //Add the CORS headers to the request to allowe the cross domain stuff.
            Utilities.AddCorsHeadersToResponse(WebOperationContext.Current);

            try
            {

                //We'll need this outside the coming try/catch block.
                EndpointDescription description;

                //Now we're going to initialize any other parts of the token that could potentially crash. 
                //We're going to do this inside another try block so that the error can be properly handed back to the client while still posting shit to the client.
                try
                {
                    //Create the Nhibernate communication session that will be carried thorughout this request.
                    token.CommunicationSession = DataAccess.NHibernateHelper.CreateSession();

                    //Now that we have a comm session, before we do anything else, let's save our token.
                    token.CommunicationSession.Save(token, token.Id);

                    //Get the IP address of the host that called us.
                    token.HostAddress = (OperationContext.Current.IncomingMessageProperties
                        [System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name] as System.ServiceModel.Channels.RemoteEndpointMessageProperty).Address;

                    //Get the args from the stream.
                    token.Args = Utilities.ConvertPostDataToDict(data);

                    //Get and validate the API Key.
                    Guid apiKey;
                    if (!Guid.TryParse(token.GetArgOrFail("apikey", "You must send an apikey") as string, out apiKey))
                        throw new ServiceException("The API key was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

                    token.ApiKey = token.CommunicationSession.Get<ApiKey>(apiKey);

                    if (token.ApiKey == null)
                        throw new ServiceException("That API Key is invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);

                    //Validate the endpoint and get the endpoint's description.
                    if (!Endpoints.TryGetValue(token.Endpoint, out description))
                        throw new ServiceException(string.Format("The endpoint '{0}' is not valid.  If you're certain this should be an endpoint and you've checked your spelling, yell at Atwood.  For further issues, please call Atwood at 505-401-7252.", token.Endpoint), ErrorTypes.Validation, System.Net.HttpStatusCode.NotFound);

                    //Is the endpoint active?
                    if (!description.IsActive)
                        throw new ServiceException(string.Format("The endpoint '{0}' has been disabled.  Please yell at Atwood for more information.", token.Endpoint), ErrorTypes.Validation, System.Net.HttpStatusCode.ServiceUnavailable);

                    token.State = MessageStates.Processed;

                    //And then print them out.
                    Communicator.PostMessageToHost("{0} | {1} | {2}\n\t\tCall Time: {3}\n\t\tProcessing Time: {4}\n\t\tHost: {5}\n\t\tApp Name: {6}".FormatS(token.Id.ToString(), token.Endpoint, token.State.ToString(),
                        token.CallTime.ToString(),
                        DateTime.Now.Subtract(token.CallTime).ToString(),
                        token.HostAddress,
                        token.ApiKey.ApplicationName), Communicator.MessagePriority.Informational);

                }
                catch (Exception e)
                {
                    Communicator.PostMessageToHost("An incoming request to invoke the endpoint, '{0}', failed at validation.  Please see the log for more information.\n\tMessage ID: {1}\n\tError Message: {2}".FormatS(token.Endpoint, token.Id, e.Message), Communicator.MessagePriority.Informational);
                    token.State = MessageStates.Failed;
                    throw e;
                }

                

                //Ok, now we know that the request is valid let's see if we need ot authenticate it.
                if (description.RequiresAuthentication)
                {
                    //We're going to do this authentication in its own try/catch so that we can show a nicer error message.
                    try
                    {
                        token = AuthenticateMessage(token);

                        //Because the session was successfully authenticated, let's go ahead and update it, since this is now the most recent time it was used, regardless if anything fails after this,
                        //at least the client tried to use the session.
                        token.AuthenticationSession.LastUsedTime = DateTime.Now;
                        token.State = MessageStates.Authenticated;

                        //And then print them out.
                        Communicator.PostMessageToHost("{0} | {1} | {2}\n\t\tCall Time: {3}\n\t\tProcessing Time: {4}\n\t\tHost: {5}\n\t\tApp Name: {6}\n\t\tSession ID: {7}".FormatS(token.Id.ToString(), token.Endpoint, token.State.ToString(),
                            token.CallTime.ToString(),
                            DateTime.Now.Subtract(token.CallTime).ToString(),
                            token.HostAddress,
                            token.ApiKey.ApplicationName,
                            token.AuthenticationSession.Id), Communicator.MessagePriority.Informational);
                    }
                    catch (Exception e)
                    {
                        Communicator.PostMessageToHost("An incoming request to invoke the endpoint, '{0}', failed at authentication.  Please see the log for more information.\n\tMessage ID: {1}\n\tError Message: {2}".FormatS(token.Endpoint, token.Id, e.Message), Communicator.MessagePriority.Informational);
                        token.State = MessageStates.Failed;
                        throw e;
                    }

                }

                try
                {
                    //Invoke the data method to which the endpoint points.
                    token = description.DataMethod(token);
                    token.State = MessageStates.Invoked;

                    //And then print them out.
                    Communicator.PostMessageToHost("{0} | {1} | {2}\n\t\tCall Time: {3}\n\t\tProcessing Time: {4}\n\t\tHost: {5}\n\t\tApp Name: {6}\n\t\tSession ID: {7}".FormatS(token.Id.ToString(), token.Endpoint, token.State.ToString(),
                        token.CallTime.ToString(),
                        DateTime.Now.Subtract(token.CallTime).ToString(),
                        token.HostAddress,
                        token.ApiKey.ApplicationName,
                        token.AuthenticationSession == null ? "null" : token.AuthenticationSession.Id.ToString()), Communicator.MessagePriority.Informational);
                }
                catch (Exception e)
                {
                    Communicator.PostMessageToHost("An incoming request to invoke the endpoint, '{0}', failed at invocation.  Please see the log for more information.\n\tMessage ID: {1}\n\tError Message: {2}".FormatS(token.Endpoint, token.Id, e.Message), Communicator.MessagePriority.Informational);
                    token.State = MessageStates.Failed;
                    throw e;
                }

                //At this point we have the data we need.  Now let's pass do our final logging.
                //TODO do logging


                //Build what will be our final return string.  We do this prior to updating the message as "handled" in case serializing the return container takes an appreciable about of time so that we don't show a false handled time.
                string finalResult = new ReturnContainer
                {
                    ErrorMessages = new List<string>(),
                    HasError = false,
                    ReturnValue = token.Result,
                    ErrorType = ErrorTypes.Null,
                    StatusCode = System.Net.HttpStatusCode.OK
                }.Serialize();

                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;

                //Now that the final logging is done and the message has been built for the client, let's release it.


                

                //Update the token in the database, indicating that we have successfully handled it.
                token.HandledTime = DateTime.Now;
                token.State = MessageStates.Handled;

                //Tell the host we finished with the message.
                List<string[]> elements = new List<string[]> { new[] { "ID", "App Name", "Host", "Call Time", "Processing Time", "Endpoint", "State", "Session ID" } };
                elements.Add(new[] { token.Id.ToString(), token.ApiKey.ApplicationName, token.HostAddress, token.CallTime.ToString(), DateTime.Now.Subtract(token.CallTime).ToString(), token.Endpoint, token.State.ToString(), token.AuthenticationSession == null ? "null" : token.AuthenticationSession.Id.ToString() });
                Communicator.PostMessageToHost(DisplayUtilities.PadElementsInLines(elements, 3), Communicator.MessagePriority.Informational);

                //Return our result.
                return finalResult;

            }
            catch (ServiceException e) //If this exception is received, then it was an expected exception that we explicitly threw.  These kinds of exceptions are ok, and are used as validation errors, authorization errors, or authentication errors that caused the message not to be processable. (yes it's a word, fuck you)
            {
                string finalResult = new ReturnContainer
                {
                    ErrorMessages = e.Message.CreateList(),
                    ErrorType = e.ErrorType,
                    HasError = true,
                    ReturnValue = null,
                    StatusCode = e.HttpStatusCode
                }.Serialize();

                WebOperationContext.Current.OutgoingResponse.StatusCode = e.HttpStatusCode;

                return finalResult;
            }
            catch (Exception e) //Any other exception is really bad.  It means something was thrown by the service or the CLR itself.  Catch this exception and alert the developers.
            {
                string finalResult = new ReturnContainer
                {
                    ErrorMessages = e.Message.CreateList(),
                    ErrorType = ErrorTypes.Fatal,
                    HasError = true,
                    ReturnValue = null,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                }.Serialize();

                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;

                return finalResult;
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
                throw new ServiceException("You must send an authentication token.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
            string authenticationToken = token.Args["authenticationtoken"] as string;

            //Alright let's try to get the session.
            var authenticationSession = token.CommunicationSession.Get<AuthenticationSession>(authenticationToken);

            if (authenticationSession == null)
                throw new ServiceException("That authentication token does not belong to an actual authenticated session.  Consider logging in so as to attain a token.", ErrorTypes.Authentication, System.Net.HttpStatusCode.BadRequest);

            //Ok so we got a session, is it valid?
            if (authenticationSession.IsExpired())
                throw new ServiceException("The session has timed out.  Please sign back in.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);

            //Since the session is in fact valid, we can go ahead and tack it onto the token.
            token.AuthenticationSession = authenticationSession;

            return token;
        }

        #endregion

    }
}
