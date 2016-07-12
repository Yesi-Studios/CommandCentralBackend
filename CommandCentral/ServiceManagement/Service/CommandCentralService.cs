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
using System.Linq.Expressions;
using CommandCentral.ClientAccess;

namespace CommandCentral.ServiceManagement.Service
{
    /// <summary>
    /// Describes the service and its implementation of the endpoints.
    /// </summary>
    [ServiceBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
    public class CommandCentralService : ICommandCentralService
    {

        #region Endpoints

        /// <summary>
        /// All endpoint calls get directed to this method where we read the endpoint the client is trying to call and then look for that endpoint to have been registered at start up.
        /// <para />
        /// The call is then decoded and the API key is validated.  Then, if the endpoint requires authentication to occur, we do that.
        /// <para /> 
        /// After authentication the call falls to the endpoint's data method where the token.Result is expected to be set.
        /// <para /> 
        /// At any point the message's state can be determined we insert it into the database (in the case of failure or success).
        /// <para />
        /// Finally, the message is released to the client using .Serialize().
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<string> InvokeGenericEndpointAsync(Stream data, string endpoint)
        {
            //Create the new message token for this request.
            MessageToken token = new MessageToken { CalledEndpoint = endpoint };

            //Tell the client we have a request.
            Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Informational);

            //Add the CORS headers to the request to allow the cross domain stuff.  We need to add this outside the try/catch block so that we can send responses to the client for an exception.
            Utilities.AddCorsHeadersToResponse(WebOperationContext.Current);

            WebOperationContext.Current.OutgoingResponse.Headers.Add(System.Net.HttpResponseHeader.ContentType, "application/json");

            try
            {
                //Get the endpoint
                ServiceEndpoint description;
                if (!ServiceManager.EndpointDescriptions.TryGetValue(token.CalledEndpoint, out description))
                {
                    token.AddErrorMessage("The endpoint you requested was not a valid endpoint. If you're certain this should be an endpoint and you've checked your spelling, yell at Atwood.  For further issues, please call Atwood at 505-401-7252.", ErrorTypes.Validation, System.Net.HttpStatusCode.NotFound);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                    return token.ConstructResponseString();
                }

                if (!description.IsActive)
                {
                    token.AddErrorMessage("The endpoint you requested is not currently available at this time.", ErrorTypes.Validation, System.Net.HttpStatusCode.ServiceUnavailable);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                    return token.ConstructResponseString();
                }

                //Create the NHibernate communication session that will be carried throughout this request.
                token.CommunicationSession = NHibernateHelper.CreateStatefulSession();

                //Get the IP address of the host that called us.
                token.HostAddress = ((RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name]).Address;

                //Set the request body and convert it into args.
                token.SetRequestBody(Utilities.ConvertStreamToString(data), true);

                //If setting the request body caused an error then we should bail here.
                if (token.HasError)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                    return token.ConstructResponseString();
                }

                //Get the apikey.
                if (!token.Args.ContainsKey("apikey"))
                    token.AddErrorMessage("You didn't send an 'apikey' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                else
                {
                    //Ok, so there is an apikey!  Is it legit?
                    Guid apiKey;
                    if (!Guid.TryParse(token.Args["apikey"] as string, out apiKey))
                        token.AddErrorMessage("The apikey parameter was not in the correct format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    else
                    {
                        //Ok, well it's a GUID.   Do we have it in the database?...
                        token.APIKey = token.CommunicationSession.Get<APIKey>(apiKey);

                        //Let's see if we caught one.
                        if (token.APIKey == null)
                            token.AddErrorMessage("Your API key was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                    }
                }

                //If the apikey was wrong, then let's bail here.
                if (token.HasError)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                    return token.ConstructResponseString();
                }

                //Alright! If we got to this point, then the message had been fully processed.  We set the message state in case the message fails.
                token.State = MessageStates.Processed;

                Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Informational);

                //Ok, now we know that the request is valid let's see if we need to authenticate it.
                if (description.EndpointMethodAttribute.RequiresAuthentication)
                {
                    AuthenticateMessage(token);

                    //If the authentication token was wrong, then let's bail here.
                    if (token.HasError)
                    {
                        WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                        return token.ConstructResponseString();
                    }

                    //Because the session was successfully authenticated, let's go ahead and update it, since this is now the most recent time it was used, regardless if anything fails after this,
                    //at least the client tried to use the session.
                    token.AuthenticationSession.LastUsedTime = DateTime.Now;
                    token.State = MessageStates.Authenticated;

                    Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Informational);
                }

                //Invoke the data method to which the endpoint points.
                description.EndpointMethod(token);
                token.State = MessageStates.Invoked;

                Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Informational);

                //Do the final handling. This involves turning the response into JSON, inserting/updating the handled token and then releasing the response.
                token.HandledTime = DateTime.Now;
                token.State = MessageStates.Handled;

                //ALright it's all done so let's go ahead and save the token.
                if (token.CommunicationSession != null)
                {
                    token.CommunicationSession.Save(token);

                    //Now we don't need the comm session anymore.
                    token.CommunicationSession.Flush();
                    token.CommunicationSession.Dispose();
                }

                Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Informational);

                //Return the final response.
                WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                return token.ConstructResponseString();
            }
            catch (Exception e)
            {
                token.AddErrorMessage(e.Message, ErrorTypes.Fatal, System.Net.HttpStatusCode.InternalServerError);

                //ALright it's all done so let's go ahead and save the token.
                if (token.CommunicationSession != null)
                {
                    token.CommunicationSession.SaveOrUpdate(token);

                    //Now we don't need the comm session anymore.
                    token.CommunicationSession.Flush();
                    token.CommunicationSession.Dispose();
                }

                //Post to the host.
                Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Critical);

                WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                return token.ConstructResponseString();
            }
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
        private static void AuthenticateMessage(MessageToken token)
        {

            //Get the authenticationtoken.
            if (!token.Args.ContainsKey("authenticationtoken"))
                token.AddErrorMessage("You didn't send an 'authenticationtoken' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
            else
            {
                //Ok, so there is an authenticationtoken!  Is it legit?
                Guid authenticationToken;
                if (!Guid.TryParse(token.Args["authenticationtoken"] as string, out authenticationToken))
                    token.AddErrorMessage("The authenticationtoken parameter was not in the correct format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                else
                {
                    //Ok, well it's a GUID.   Do we have it in the database?...
                    var authenticationSession = token.CommunicationSession.Get<AuthenticationSession>(authenticationToken);

                    //Did we get a session and if so is it valid?
                    if (authenticationSession == null)
                        token.AddErrorMessage("That authentication token does not belong to an actual authenticated session.  Consider logging in so as to attain a token.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                    else if (authenticationSession.IsExpired())
                        token.AddErrorMessage("The session has timed out.  Please sign back in.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                    else
                        token.AuthenticationSession = authenticationSession;
                }
            }
        }

        #endregion

    }
}
