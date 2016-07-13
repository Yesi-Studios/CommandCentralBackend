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

        #region Options Request

        /// <summary>
        /// Response to an options request.
        /// <para />
        /// An options request is a pre-flight request sent by browsers to ask what the service allows and doesn't allow.  Our response is simply our standard headers package.
        /// </summary>
        public void GetOptions()
        {
            //Add the headers, this will adequately reply to the options request.
            AddHeadersToOutgoingResponse(WebOperationContext.Current);

            //Tell the host we received a pre flight.
            Communicator.PostMessageToHost("Received Preflight Request", Communicator.MessageTypes.Informational);
        }

        #endregion

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
            //The token we're going to use for this request.
            MessageToken token = new MessageToken();

            try
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {

                    try
                    {
                        //Create the new message token for this request.
                        token.CalledEndpoint = endpoint;

                        //Tell the client we have a request.
                        Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Informational);

                        //Add the headers to the response.
                        AddHeadersToOutgoingResponse(WebOperationContext.Current);

                        //Get the endpoint
                        ServiceEndpoint description;
                        if (!ServiceManager.EndpointDescriptions.TryGetValue(token.CalledEndpoint, out description))
                        {
                            token.AddErrorMessage("The endpoint you requested was not a valid endpoint. If you're certain this should be an endpoint " +
                                "and you've checked your spelling, yell at the developers.  For further issues, please call the developers at {0}.".FormatS(Config.ContactDetails.DEV_PHONE_NUMBER), ErrorTypes.Validation, System.Net.HttpStatusCode.NotFound);
                            WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                            return token.ConstructResponseString();
                        }

                        //If the endpoint was retrieved successfully, then assign it here.
                        token.EndpointDescription = description;

                        //If the endpoint is inactive, return an error message.
                        if (!token.EndpointDescription.IsActive)
                        {
                            token.AddErrorMessage("The endpoint you requested is not currently available at this time.", ErrorTypes.Validation, System.Net.HttpStatusCode.ServiceUnavailable);
                            WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                            return token.ConstructResponseString();
                        }

                        //Get the IP address of the host that called us.  We can't really do this in a separate static method because if I did, I would likely put that in the Utils library,
                        //but if you put it there then for some reason it breaks the operation context and it can't find the current request. :(
                        token.HostAddress = ((RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name]).Address;

                        //Set the request body and convert it into args.
                        token.SetRequestBody(Utilities.ConvertStreamToString(data), true);

                        //If setting the request body caused an error then we should bail here.
                        if (token.HasError)
                        {
                            WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                            return token.ConstructResponseString();
                        }

                        //Ok so the message still needs to go through some validation but that validation requires database stuff.  
                        //So at this point I'm going to create the session.

                        #region A message to future wayward souls

                        //This session is used in this scope only to do authentication work on the state of the message, NOT THE REQUEST ITSELF.
                        //Please good god DO NOT pass this session into the endpoint methods.  The caching issues are truly amazing and the concurrency concerns... will make you concerned.
                        //No really, please god don't do it.  Every method gets its own session.
                        //Do you think it would be a good idea to pass a session on the message token into the request and use that as your unit of work?
                        //Cute, I did too.  I promise you it doesn't work.  But if you want to try it, knock your socks off - long hair don't care.  Actually I do.  Turn back now.  If you keep going, read this:
                        /*
                         * Through me you go to the grief wracked city;
                         * Through me you go to everlasting pain;
                         * Through me you go a pass among lost souls.
                         * Abandon all hope - Ye who enter here.
                         */
                        //As an aside, creating sessions from the factory is very cheap, in case that helps inform your decision not to waste your life.

                        //As a security note: this session should always be segragated from the operations the client is asking for. 
                        //For example, if a client wants their permissions changed, those permissions will change at the end of the request because this session will still hold the old permissions.
                        //Sorry I went on so long - this has been the bane of my existence for months.

                        #endregion

                        //Get the apikey.
                        if (!token.Args.ContainsKey(Config.ParamNames.API_KEY))
                            token.AddErrorMessage("You didn't send an '{0}' parameter.".FormatS(Config.ParamNames.API_KEY), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        else
                        {
                            //Ok, so there is an apikey!  Is it legit?
                            Guid apiKey;
                            if (!Guid.TryParse(token.Args[Config.ParamNames.API_KEY] as string, out apiKey))
                                token.AddErrorMessage("The '{0}' parameter was not in the correct format.".FormatS(Config.ParamNames.API_KEY), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            else
                            {
                                //Ok, well it's a GUID.   Do we have it in the database?...
                                token.APIKey = session.Get<APIKey>(apiKey);

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
                            AuthenticateMessage(token, session);

                            //If the authentication token was wrong, then let's bail here.
                            if (token.HasError)
                            {
                                WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                                return token.ConstructResponseString();
                            }

                            //Because the session was successfully authenticated, let's go ahead and update it, since this is now the most recent time it was used, regardless if anything fails after this,
                            //at least the client tried to use the session.
                            token.AuthenticationSession.LastUsedTime = token.CallTime;
                            token.State = MessageStates.Authenticated;

                            Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Informational);
                        }

                        //Invoke the data method to which the endpoint points. Point
                        description.EndpointMethod(token);
                        token.State = MessageStates.Invoked;

                        Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Informational);

                        //Do the final handling. This involves turning the response into JSON, inserting/updating the handled token and then releasing the response.
                        token.HandledTime = DateTime.Now;
                        token.State = MessageStates.Handled;

                        //Alright it's all done so let's go ahead and save the token.
                        session.Save(token);

                        Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Informational);

                        if (token.StatusCode != System.Net.HttpStatusCode.OK)
                            throw new Exception("A request made it to the end of handling; however, its status was not OK.");

                        //Return the final response.
                        WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;

                        string finalResponse = token.ConstructResponseString();

                        //Everything good?  Commit the transaction right before we release.
                        transaction.Commit();

                        return finalResponse;
                    }
                    catch (Exception e) //if we can catch the exception here don't rethrow it.  We can handle it here by logging the message and sending back to the client.
                    {
                        //If an issue occurred very first thing we do is roll anything back we may have done.  It shouldn't actually be anything unless we failed right at the end but whatever.
                        transaction.Rollback();

                        //Add the error message
                        token.AddErrorMessage("A fatal occurred within the backend service.  We are extremely sorry for this inconvenience." +
                            "  The developers have been alerted and a trained monkey(s) has been dispatched.", ErrorTypes.Fatal, System.Net.HttpStatusCode.InternalServerError);

                        //Save the token
                        session.Save(token);

                        EmailHelper.SendFatalErrorEmail(token, e);

                        //Tell the host what happened.
                        Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Critical);

                        //Set the outgoing status code and then release.
                        WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                        return token.ConstructResponseString();
                    }
                }
            }
            catch (Exception e) //Exceptions that make it to here were caused during creation of the communication session.  We should give the client something generic but also inform the developers.
            {
                //Give the token the error message and then release it.  Just like the above catch block. 
                token.AddErrorMessage("An error occurred while trying to create a database session.  The database may be inaccessible right now.", ErrorTypes.Fatal, System.Net.HttpStatusCode.InternalServerError);

                EmailHelper.SendFatalErrorEmail(token, e);

                //We can't save it cause we have no session so just post the message and then release.
                Communicator.PostMessageToHost(token.ToString(), Communicator.MessageTypes.Critical);

                //Set the outgoing status code and then release.
                WebOperationContext.Current.OutgoingResponse.StatusCode = token.StatusCode;
                return token.ConstructResponseString();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds any headers we want to the response.
        /// </summary>
        /// <param name="current"></param>
        private static void AddHeadersToOutgoingResponse(WebOperationContext current)
        {
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Accept,Authorization");
            current.OutgoingResponse.Headers.Add(System.Net.HttpResponseHeader.ContentType, "application/json");
        }

        /// <summary>
        /// Authenticates the message by using the authentication token from the args list to retrieve the client's session.  
        /// <para />
        /// This method also ensures that the session has not become inactive
        /// </summary>
        /// <param name="token"></param>
        /// <param name="session">The session on which to do our authentication work.</param>
        /// <returns></returns>
        private static void AuthenticateMessage(MessageToken token, NHibernate.ISession session)
        {

            //Get the authenticationtoken.
            if (!token.Args.ContainsKey(Config.ParamNames.AUTHENTICATION_TOKEN))
                token.AddErrorMessage("You didn't send an '{0}' parameter.".FormatS(Config.ParamNames.AUTHENTICATION_TOKEN), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
            else
            {
                //Ok, so there is an authenticationtoken!  Is it legit?
                Guid authenticationToken;
                if (!Guid.TryParse(token.Args[Config.ParamNames.AUTHENTICATION_TOKEN] as string, out authenticationToken))
                    token.AddErrorMessage("The '{0}' parameter was not in the correct format.".FormatS(Config.ParamNames.AUTHENTICATION_TOKEN), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                else
                {
                    //Ok, well it's a GUID.   Do we have it in the database?...
                    var authenticationSession = session.Get<AuthenticationSession>(authenticationToken);

                    //Did we get a session and if so is it valid?
                    if (authenticationSession == null)
                        token.AddErrorMessage("That authentication token does not belong to an actual authenticated session.  Consider logging in so as to attain a token.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                    else if (authenticationSession.IsValid())
                        token.AddErrorMessage("The session has timed out or is no longer valid.  Please sign back in.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                    else
                        token.AuthenticationSession = authenticationSession;
                }
            }
        }

        #endregion

    }
}
