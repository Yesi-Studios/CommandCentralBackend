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
using CCServ.DataAccess;
using System.Linq.Expressions;
using CCServ.ClientAccess;
using CCServ.Logging;
using Polly;
using Humanizer;
using System.Net;

namespace CCServ.ServiceManagement.Service
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
            try
            {
                //Add the headers, this will adequately reply to the options request.
                AddHeadersToOutgoingResponse(WebOperationContext.Current);

                //Tell the host we received a pre flight.
                Log.Debug("Received Preflight Request");
            }
            catch (Exception e)
            {
                Log.Exception(e, "An error occurred during the pre flight options request.");

                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            
        }

        #endregion

        #region Main Endpoint / Client Entry Point

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
        public string InvokeGenericEndpointAsync(Stream data, string endpoint)
        {
            AddHeadersToOutgoingResponse(WebOperationContext.Current);

            //The token we're going to use for this request.
            MessageToken token = new MessageToken();

            try
            {
                using (var session = NHibernateHelper.CreateStatefulSession())
                {
                    //First up, we're going to save the token in its own transaction.  
                    //We just want to get it recorded in the database that we received a request before moving on with anything else.
                    using (var transaction = session.BeginTransaction())
                    {
                        try
                        {
                            session.Save(token);

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }

                    //Now that we've saved the token, let's continue on with other work.
                    using (var transaction = session.BeginTransaction())
                    {

                        try
                        {
                            //Create the new message token for this request.
                            token.CalledEndpoint = endpoint;

                            //Tell the logs we have a client.
                            Log.Debug(token.ToString());

                            //Get the endpoint
                            if (!ServiceManager.EndpointDescriptions.TryGetValue(token.CalledEndpoint, out ServiceEndpoint description))
                                throw new CommandCentralException("The endpoint you requested was not a valid endpoint. If you're certain this should be an endpoint " +
                                    "and you've checked your spelling, yell at the developers.  For further issues, please contact the developers at {0}.".FormatS(Email.EmailInterface.CCEmailMessage.DeveloperAddress.Address),
                                    ErrorTypes.Validation);

                            //If the endpoint was retrieved successfully, then assign it here.
                            token.EndpointDescription = description;

                            //If the endpoint is inactive, return an error message.
                            if (!token.EndpointDescription.IsActive)
                                throw new CommandCentralException("The endpoint you requested is not currently available at this time.", ErrorTypes.Validation);

                            //Get the IP address of the host that called us.  We can't really do this in a separate static method because if I did, I would likely put that in the Utils library,
                            //but if you put it there then for some reason it breaks the operation context and it can't find the current request. :(
                            token.HostAddress = ((RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name]).Address;

                            //Set the request body and convert it into args.
                            token.SetArgs(Utilities.ConvertStreamToString(data));

                            //Get the apikey.
                            if (!token.Args.ContainsKey("apikey"))
                                throw new CommandCentralException("You didn't send an 'apikey' parameter.", ErrorTypes.Validation);

                            //Ok, so there is an apikey!  Is it legit?
                            if (!Guid.TryParse(token.Args["apikey"] as string, out Guid apiKey))
                                throw new CommandCentralException("The 'apikey' parameter was not in the correct format.", ErrorTypes.Validation);

                            //Ok, well it's a GUID.   Do we have it in the database?...
                            token.APIKey = session.Get<APIKey>(apiKey);

                            //Let's see if we caught one.
                            if (token.APIKey == null)
                                throw new CommandCentralException("Your apikey was not valid.", ErrorTypes.Validation);

                            Log.Debug(token.ToString());

                            //Ok, now we know that the request is valid, let's see if we need to authenticate it.
                            if (description.EndpointMethodAttribute.RequiresAuthentication)
                            {
                                AuthenticateMessage(token, session);
                                
                                //Because the session was successfully authenticated, let's go ahead and update it, since this is now the most recent time it was used, regardless if anything fails after this,
                                //at least the client tried to use the session.
                                token.AuthenticationSession.LastUsedTime = token.CallTime;

                                Log.Debug(token.ToString());
                            }

                            //Invoke the data method of the endpoint.
                            description.EndpointMethod(token);

                            Log.Debug(token.ToString());

                            token.HandledTime = DateTime.UtcNow;

                            //Alright it's all done so let's go ahead and save the token.
                            session.SaveOrUpdate(token);

                            Log.Debug(token.ToString());

                            bool failedAtLeastOnce = false;
                            //Everything good?  Commit the transaction right before we release.  Included some handling for deadlocks.
                            var result = Policy
                                .Handle<NHibernate.ADOException>()
                                .WaitAndRetry(2, count => TimeSpan.FromSeconds(1), (e, waitDuration, retryCount, context) =>
                                {
                                    failedAtLeastOnce = true;
                                    Log.Warning("A session transaction failed to commit.  Retry count: {0}".FormatWith(retryCount), token);
                                })
                                .ExecuteAndCapture(() =>
                                {
                                    transaction.Commit();
                                });

                            if (failedAtLeastOnce && result.Outcome == OutcomeType.Successful)
                            {
                                Log.Warning("A session transaction failed to commit but succeeded after reattempt.", token);
                            }
                            else if (result.Outcome == OutcomeType.Failure)
                            {
                                throw result.FinalException;
                            }

                            return new ReturnContainer
                            {
                                StatusCode = HttpStatusCode.OK,
                                ErrorType = ErrorTypes.Null,
                                ReturnValue = token.Result
                            }.Serialize();
                        }
                        catch (AggregateException e) when (e.InnerExceptions.All(x => x.GetType() == typeof(CommandCentralException)))
                        {
                            if (e.InnerExceptions.GroupBy(x => ((CommandCentralException)x).ErrorType).Count() != 1)
                                throw new Exception("An aggregate exception was thrown with multiple error types.");

                            session.SaveOrUpdate(token);
                            transaction.Commit();

                            WebOperationContext.Current.OutgoingResponse.StatusCode = (HttpStatusCode)((CommandCentralException)e.InnerExceptions.First()).ErrorType.GetMatchStatusCode();

                            return new ReturnContainer
                            {
                                ErrorMessages = e.InnerExceptions.Select(x => x.Message).ToList(),
                                ErrorType = ((CommandCentralException)e.InnerExceptions.First()).ErrorType,
                                ReturnValue = null,
                                StatusCode = WebOperationContext.Current.OutgoingResponse.StatusCode
                            }.Serialize();
                        }
                        catch (CommandCentralException e)
                        {
                            session.SaveOrUpdate(token);
                            transaction.Commit();

                            WebOperationContext.Current.OutgoingResponse.StatusCode = e.ErrorType.GetMatchStatusCode();

                            return new ReturnContainer
                            {
                                ErrorMessages = new List<string> { e.Message },
                                ErrorType = e.ErrorType,
                                ReturnValue = null,
                                StatusCode = WebOperationContext.Current.OutgoingResponse.StatusCode
                            }.Serialize();
                        }
                        catch (Exception e) //if we can catch the exception here don't rethrow it.  We can handle it here by logging the message and sending back to the client.
                        {
                            //Save the token
                            session.SaveOrUpdate(token);

                            transaction.Commit();

                            //Log what happened.
                            Log.Exception(e, "A fatal, unknown error occurred in the backend service.", token);

                            //Set the outgoing status code and then release.
                            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;

                            return new ReturnContainer
                            {
                                ErrorMessages = new List<string> { "A fatal error occurred within the backend service.  We are extremely sorry for this inconvenience." +
                                    "  The developers have been alerted and a trained monkey(s) has been dispatched." },
                                ErrorType = ErrorTypes.Fatal,
                                ReturnValue = null,
                                StatusCode = System.Net.HttpStatusCode.InternalServerError
                            }.Serialize();
                        }
                    }
                }
            }
            catch (Exception e) //Exceptions that make it to here were caused during creation of the communication session.  We should give the client something generic but also inform the developers.
            {
                Log.Exception(e, "An error occurred while trying to create a database session.", token);

                //Set the outgoing status code and then release.
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;

                return new ReturnContainer
                {
                    ErrorMessages = new List<string> { "It appears our database is currently offline.  We'll be back up and running shortly!" },
                    ErrorType = ErrorTypes.Fatal,
                    ReturnValue = null,
                    StatusCode = HttpStatusCode.InternalServerError
                }.Serialize();
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
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Accept,Authorization");
            current.OutgoingResponse.Headers.Add(HttpResponseHeader.ContentType, "application/json");
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
            if (!token.Args.ContainsKey("authenticationtoken"))
                throw new CommandCentralException("You failed to send an 'authenticationtoken' parameter.", ErrorTypes.Validation);

            //Ok, so there is an authenticationtoken!  Is it legit?
            if (!Guid.TryParse(token.Args["authenticationtoken"] as string, out Guid authenticationToken))
                throw new CommandCentralException("The 'authenticationtoken' parameter was not in the correct format.", ErrorTypes.Validation);

            //Ok, well it's a GUID.   Do we have it in the database?...
            var authenticationSession = session.Get<AuthenticationSession>(authenticationToken) ??
                throw new CommandCentralException("That authentication token does not belong to an actual authenticated session.  " +
                "Consider logging in so as to attain a token.", ErrorTypes.Authentication);

            if (!authenticationSession.IsValid())
                throw new CommandCentralException("The session has timed out or is no longer valid.  Please sign back in.", ErrorTypes.Authentication);

            token.AuthenticationSession = authenticationSession;
            //HACK
            token.AuthenticationSession.Person.PermissionGroups = Authorization.AuthorizationUtilities.GetPermissionGroupsFromNames(token.AuthenticationSession.Person.PermissionGroupNames, true);
        }

        #endregion
    }
}
