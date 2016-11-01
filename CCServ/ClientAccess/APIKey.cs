using System;
using FluentNHibernate.Mapping;
using CCServ.Authorization;
using System.Linq;
using AtwoodUtils;
using CCServ.Logging;

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

        #region Startup Methods

        /// <summary>
        /// Scans the API Keys in the database and ensures that the primary one exists.  Also prints all the API Keys we have.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 2)]
        private static void SetupAPIKeys(CLI.Options.LaunchOptions launchOptions)
        {
            Log.Info("Scanning API Keys...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    if (session.Get<APIKey>(_primaryAPIKey.Id) == null)
                    {
                        Log.Warning("The primary API key was not found!  Adding it now...");
                        session.Save(_primaryAPIKey);
                        Log.Info("Primary API key was added.  Key : {0} | Name : {1}".FormatS(_primaryAPIKey.Id, _primaryAPIKey.ApplicationName));
                    }

                    //Now tell the client how many we have.
                    var apiKeys = session.QueryOver<APIKey>().List();

                    Log.Info("{0} API key(s) found for the application(s) {1}".FormatS(apiKeys.Count, String.Join(",", apiKeys.Select(x => String.Format("'{0}'", x.ApplicationName)))));

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
