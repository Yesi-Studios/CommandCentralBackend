using System;
using FluentNHibernate.Mapping;
using CommandCentral.Authorization;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Describes a single API Key.
    /// </summary>
    public class APIKey
    {

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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.ApplicationName).Unique().Length(40).Not.LazyLoad();
            }
        }

    }
}
