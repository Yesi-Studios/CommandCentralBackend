using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.DataAccess;

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
        /// The list of all _endpointDescriptions that are exposed to the client.
        /// </summary>
        private static readonly ConcurrentDictionary<string, EndpointDescription> _endpointDescriptions = new ConcurrentDictionary<string, EndpointDescription>();

        /// <summary>
        /// Gets the exposed endpoints.
        /// </summary>
        public static ConcurrentDictionary<string, EndpointDescription> EndpointDescriptions
        {
            get { return _endpointDescriptions; }
        }

        /// <summary>
        /// Static constructor that builds the _endpointDescriptions.  This is how we register new _endpointDescriptions.
        /// </summary>
        static CommandCentralService()
        {
            Entities.Person.EndpointDescriptions.ToList().ForEach(x => _endpointDescriptions.AddOrUpdate(x.Key, x.Value,
                (key, value) =>
                {
                    throw new Exception();
                }));

            ReferenceListItemBase.EndpointDescriptions.ToList().ForEach(x => _endpointDescriptions.AddOrUpdate(x.Key, x.Value,
                (key, value) =>
                {
                    throw new Exception();
                }));

            Entities.ReferenceLists.Command.EndpointDescriptions.ToList().ForEach(x => _endpointDescriptions.AddOrUpdate(x.Key, x.Value,
                (key, value) =>
                {
                    throw new Exception();
                }));

            Entities.VersionInformation.EndpointDescriptions.ToList().ForEach(x => _endpointDescriptions.AddOrUpdate(x.Key, x.Value,
                (key, value) =>
                {
                    throw new Exception();
                }));
        }

        #endregion


        /// <summary>
        /// Allows for dynamic invocation of endpoints by using the EndpointsDescription dictionary to whitelist the _endpointDescriptions.
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
            Communicator.PostMessageToHost("{0} | {1} | {2}\n\t\tCall Time: {3}".FormatS(token.Id.ToString(), token.Endpoint, token.State.ToString(), token.CallTime.ToString(CultureInfo.InvariantCulture)), Communicator.MessagePriority.Informational);

            //Add the CORS headers to the request to allow the cross domain stuff.
            Utilities.AddCorsHeadersToResponse(WebOperationContext.Current);

            try
            {

                //We'll need this outside the coming try/catch block.
                EndpointDescription description;

                //Now we're going to initialize any other parts of the token that could potentially crash. 
                //We're going to do this inside another try block so that the error can be properly handed back to the client while still posting shit to the client.
                try
                {
                    //Create the NHibernate communication session that will be carried throughout this request.
                    token.CommunicationSession = NHibernateHelper.CreateSession();

                    //Get the IP address of the host that called us.
                    token.HostAddress = ((RemoteEndpointMessageProperty) OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name]).Address;

                    //Here we keep the raw json in a separate variable before handing it to the token.
                    //This is because the token's property is not suitable for doing data processing because it truncates data to meet database logging requirements.
                    string rawJson = Utilities.ConvertStreamToString(data);
                    token.RawJSON = rawJson;

                    //Attempt to convert the raw json into a dictionary.
                    token.Args = rawJson.Deserialize<Dictionary<string, object>>();

                    if (token.Args == null)
                        throw new ServiceException("Your request body was in the wrong form.  It must be in a list of key/value pairs, or a dictionary.  Keys must be strings and values can be any object.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

                    //Get and validate the API Key.
                    Guid apiKey;
                    if (!Guid.TryParse(token.GetArgOrFail("apikey", "You must send an apikey") as string, out apiKey))
                        throw new ServiceException("The API key was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

                    token.ApiKey = token.CommunicationSession.Get<ApiKey>(apiKey);

                    if (token.ApiKey == null)
                        throw new ServiceException("That API Key is invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);

                    //Validate the endpoint and get the endpoint's description.
                    if (!_endpointDescriptions.TryGetValue(token.Endpoint, out description))
                        throw new ServiceException(string.Format("The endpoint '{0}' is not valid.  If you're certain this should be an endpoint and you've checked your spelling, yell at Atwood.  For further issues, please call Atwood at 505-401-7252.", token.Endpoint), ErrorTypes.Validation, System.Net.HttpStatusCode.NotFound);

                    //Is the endpoint active?
                    if (!description.IsActive)
                        throw new ServiceException(string.Format("The endpoint '{0}' has been disabled.  Please yell at Atwood for more information.", token.Endpoint), ErrorTypes.Validation, System.Net.HttpStatusCode.ServiceUnavailable);

                    token.State = MessageStates.Processed;

                    //And then print them out.
                    Communicator.PostMessageToHost("{0} | {1} | {2}\n\t\tCall Time: {3}\n\t\tProcessing Time: {4}\n\t\tHost: {5}\n\t\tApp Name: {6}".FormatS(token.Id.ToString(), token.Endpoint, token.State.ToString(),
                        token.CallTime.ToString(CultureInfo.InvariantCulture),
                        DateTime.Now.Subtract(token.CallTime).ToString(),
                        token.HostAddress,
                        token.ApiKey.ApplicationName), Communicator.MessagePriority.Informational);

                }
                catch (Exception)
                {
                    token.State = MessageStates.FailedAtProcessing;
                    throw;
                }

                

                //Ok, now we know that the request is valid let's see if we need to authenticate it.
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
                            token.CallTime.ToString(CultureInfo.InvariantCulture),
                            DateTime.Now.Subtract(token.CallTime).ToString(),
                            token.HostAddress,
                            token.ApiKey.ApplicationName,
                            token.AuthenticationSession.Id), Communicator.MessagePriority.Informational);
                    }
                    catch (Exception e)
                    {
                        token.State = MessageStates.FailedAtAuthentication;
                        throw;
                    }

                }

                try
                {
                    //Invoke the data method to which the endpoint points.
                    token = description.DataMethod(token);
                    token.State = MessageStates.Invoked;

                    //And then print them out.
                    Communicator.PostMessageToHost("{0} | {1} | {2}\n\t\tCall Time: {3}\n\t\tProcessing Time: {4}\n\t\tHost: {5}\n\t\tApp Name: {6}\n\t\tSession ID: {7}".FormatS(token.Id.ToString(), token.Endpoint, token.State.ToString(),
                        token.CallTime.ToString(CultureInfo.InvariantCulture),
                        DateTime.Now.Subtract(token.CallTime).ToString(),
                        token.HostAddress,
                        token.ApiKey.ApplicationName,
                        token.AuthenticationSession == null ? "null" : token.AuthenticationSession.Id.ToString()), Communicator.MessagePriority.Informational);
                }
                catch (Exception)
                {
                    token.State = MessageStates.FailedAtInvocation;
                    throw;
                }

                //Do the final handling. This involves turning the response into JSON, inserting/updating the handled token and then releasing the response.
                try
                {
                    //Build what will be our final return string.  To do this we just serialize the response.
                    string finalResult = new ReturnContainer
                    {
                        ErrorMessages = new List<string>(),
                        HasError = false,
                        ReturnValue = token.Result,
                        ErrorType = ErrorTypes.Null,
                        StatusCode = System.Net.HttpStatusCode.OK
                    }.Serialize();

                    Debug.Assert(WebOperationContext.Current != null, "WebOperationContext.Current != null");
                    WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;

                    //We can't use the result JSON directly because it is limited to 9000 characters.
                    token.ResultJSON = finalResult;

                    //Update the token in the database, indicating that we have successfully handled it.
                    token.HandledTime = DateTime.Now;
                    token.State = MessageStates.Handled;

                    //The stage is set.  Insert the token before we tell the host its been handled.
                    token.CommunicationSession.SaveOrUpdate(token);

                    //Clear our the comm session
                    token.CommunicationSession.Flush();
                    token.CommunicationSession.Dispose();

                    //Tell the host we finished with the message.
                    List<string[]> elements = new List<string[]>
                    {
                        new[] {"ID", "App Name", "Host", "Call Time", "Processing Time", "Endpoint", "State", "Session ID"},
                        new[]
                        {
                            token.Id.ToString(), token.ApiKey.ApplicationName, token.HostAddress,
                            token.CallTime.ToString(CultureInfo.InvariantCulture),
                            DateTime.Now.Subtract(token.CallTime).ToString(), token.Endpoint, token.State.ToString(),
                            token.AuthenticationSession == null ? "null" : token.AuthenticationSession.Id.ToString()
                        }
                    };
                    Communicator.PostMessageToHost(DisplayUtilities.PadElementsInLines(elements, 3), Communicator.MessagePriority.Informational);

                    //release our JSON result.
                    return finalResult;
                }
                catch (Exception)
                {
                    token.State = MessageStates.FailedAtFinalHandling;
                    throw;
                }
            }
            catch (ServiceException e) //If this exception is received, then it was an expected exception that we explicitly threw.  These kinds of exceptions are ok, and are used as validation errors, authorization errors, or authentication errors that caused the message not to be processable. (yes it's a word, fuck you)
            {
                //Build what we're going to send to the client.
                string finalResult = new ReturnContainer
                {
                    ErrorMessages = e.Message.CreateList(),
                    ErrorType = e.ErrorType,
                    HasError = true,
                    ReturnValue = null,
                    StatusCode = e.HttpStatusCode
                }.Serialize();

                //Set status code to whatever the error message gave us.
                Debug.Assert(WebOperationContext.Current != null, "WebOperationContext.Current != null");
                WebOperationContext.Current.OutgoingResponse.StatusCode = e.HttpStatusCode;

                //Update the token.
                token.ResultJSON = finalResult;

                //Update/Save the token before we leave or tell the client we're leaving.
                token.CommunicationSession.SaveOrUpdate(token);

                //Clear our the comm session
                token.CommunicationSession.Flush();
                token.CommunicationSession.Dispose();

                Communicator.PostMessageToHost("ERROR: An incoming request to invoke the endpoint, '{0}', failed.  Please see the log for more information.\n\tMessage ID: {1}\n\tFailure Location: {2}\n\tError Message: {3}".FormatS(token.Endpoint, token.Id, token.State.ToString(), e.Message), Communicator.MessagePriority.Informational);

                return finalResult;
            }
            catch (Exception e) //Any other exception is really bad.  It means something was thrown by the service or the CLR itself.  Catch this exception and alert the developers.
            {
                token.State = MessageStates.FatalError;

                //Build what we're going to send to the client.
                string finalResult = new ReturnContainer
                {
                    ErrorMessages = "Well... this is awkward.  Command Central suffered a serious, fatal error.  We are extremely sorry about this.  \n\n\tAn email, text message, and smoke signals have been sent to the developers with the details of this error.  \n\n\tYou'll most likely be contacted shortly if we need further information about this crash such as what you were doing in the application and the order in which you did it.  Any help you can provide us will be appreciated.".CreateList(),
                    ErrorType = ErrorTypes.Fatal,
                    HasError = true,
                    ReturnValue = null,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                }.Serialize();

                //Set status code to whatever the error message gave us.
                Debug.Assert(WebOperationContext.Current != null, "WebOperationContext.Current != null");
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;

                //Update the token.
                token.ResultJSON = finalResult;

                //Send the developers the email informing them of a fatal error.
                EmailHelper.SendFatalErrorEmail(token, e);

                //Update/Save the token before we leave or tell the client we're leaving.
                token.CommunicationSession.SaveOrUpdate(token);

                //Clear our the comm session
                token.CommunicationSession.Flush();
                token.CommunicationSession.Dispose();

                Communicator.PostMessageToHost("FATAL ERROR: An incoming request to invoke the endpoint, '{0}', failed.  Please see the log for more information.\n\tMessage ID: {1}\n\tFailure Location: {2}\n\tError Message: {3}".FormatS(token.Endpoint, token.Id, token.State.ToString(), e.Message), Communicator.MessagePriority.Informational);

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
                if (!_endpointDescriptions.TryGetValue(endpoint, out endpointDescription))
                {
                    result = string.Format("The endpoint, '{0}', was not valid.  You can not request its documentation.  Try checking your spelling.", endpoint);
                }
                else
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string templateName = "CommandCentral.Resources.Documentation.DocumentationTemplate.html";

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
        /// Returns documentation for all _endpointDescriptions.
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
            string templateName = "CommandCentral.Resources.Documentation.AllEndpointsTemplate.html";

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

            string endpointBuild = "";

            _endpointDescriptions.ToList().OrderBy(x => x.Key).ToList().ForEach(x =>
            {
                endpointBuild += string.Format(endpointTemplate, x.Key) + Environment.NewLine;
            });

            byte[] resultBytes = Encoding.UTF8.GetBytes(string.Format(templatePage, endpointBuild));
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
            //Get the authentication token.
            Guid authenticationToken;
            if (!Guid.TryParse(token.GetArgOrFail("authenticationtoken", "You must send an authentication token - this endpoint requires authentication.") as string, out authenticationToken))
                throw new ServiceException("The authentication token you sent was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);

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
