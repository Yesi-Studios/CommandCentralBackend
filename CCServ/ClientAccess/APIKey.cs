using System;
using FluentNHibernate.Mapping;
using CCServ.Authorization;
using System.Linq;
using AtwoodUtils;

namespace CCServ.ClientAccess
{
    /// <summary>
    /// Describes a single API Key.
    /// </summary>
    public class APIKey
    {
        /// <summary>
        /// This is the expected primary api key that should exist at all times in the database.
        /// </summary>
        private static APIKey _primaryAPIKey = new APIKey
        {
            ApplicationName = "Command Central Official Frontend",
            Id = Guid.Parse("90FDB89F-282B-4BD6-840B-CEF597615728")
        };

        #region Properties

        /// <summary>
        /// The unique Id of this API Key.  This is also the API Key itself.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of the application to which this API Key was unsigned.
        /// </summary>
        public virtual string ApplicationName { get; set; }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all API keys and the application names that correspond with them.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadAPIKeys", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadAPIKeys(MessageToken token)
        {
            //Just make sure the client is logged in.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view API keys.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("You don't have permission to view API keys - you must be a developer.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Client has permission, show them the api keys and the names.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                token.SetResult(session.QueryOver<APIKey>().List());
            }
        }

        #endregion

        #region Startup Methods

        /// <summary>
        /// Scans the API Keys in the database and ensures that the primary one exists.  Also prints all the API Keys we have.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 2)]
        private static void SetupAPIKeys(CLI.Options.LaunchOptions launchOptions)
        {
            Logger.LogInformation("Scanning API Keys...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    if (session.Get<APIKey>(_primaryAPIKey.Id) == null)
                    {
                        Logger.LogWarning("The primary API key was not found!  Adding it now...");
                        session.Save(_primaryAPIKey);
                        Logger.LogInformation("Primary API key was added.  Key : {0} | Name : {1}".FormatS(_primaryAPIKey.Id, _primaryAPIKey.ApplicationName));
                    }

                    //Now tell the client how many we have.
                    var apiKeys = session.QueryOver<APIKey>().List();

                    Logger.LogInformation("{0} API key(s) found for the application(s) {1}".FormatS(apiKeys.Count, String.Join(",", apiKeys.Select(x => String.Format("'{0}'", x.ApplicationName)))));

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        /// <summary>
        /// Provides mapping declarations to the database for the API Key.
        /// </summary>
        public class ApiKeyMap : ClassMap<APIKey>
        {
            /// <summary>
            /// Maps the API Key to the database.
            /// </summary>
            public ApiKeyMap()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.ApplicationName).Unique().Length(40).Not.LazyLoad();
            }
        }

    }
}
