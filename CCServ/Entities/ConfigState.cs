using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.CustomDBTypes;
using FluentNHibernate.Mapping;
using AtwoodUtils;

namespace CCServ.Entities
{
    /// <summary>
    /// Stores a config state and maps a config state to the database.
    /// </summary>
    public class ConfigState
    {
        /// <summary>
        /// This config state's Id.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of the config state.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The address at which the DOD's SMTP server can be found.  This is the address through which we should send emails.
        /// </summary>
        public virtual string DODSMTPAddress { get; set; }

        /// <summary>
        /// The email address of the developer distro from which all emails should be sent and CCed.
        /// </summary>
        public virtual string DeveloperDistroAddress { get; set; }

        /// <summary>
        /// The display name for the developer distro.
        /// </summary>
        public virtual string DeveloperDistroDisplayName { get; set; }
        
        /// <summary>
        /// Atwood's email address.
        /// </summary>
        public virtual string AtwoodGmailAddress { get; set; }
        
        /// <summary>
        /// McLean's email address.
        /// </summary>
        public virtual string McLeanGmailAddress { get; set; }

        /// <summary>
        /// The email host of a DOD email address.
        /// </summary>
        public virtual string DODEmailHost { get; set; }

        /// <summary>
        /// The time at which a muster should rollover.
        /// </summary>
        public virtual Time MusterRolloverTime { get; set; }

        /// <summary>
        /// The time by which the muster is expected to have been completed.
        /// </summary>
        public virtual Time MusterDueTime { get; set; }

        /// <summary>
        /// Indicates if the muster is in a finalized state.
        /// </summary>
        public virtual bool IsMusterFinalized { get; set; }

        /// <summary>
        /// The max age of a profile lock.
        /// </summary>
        public virtual TimeSpan ProfileLockMaxAge { get; set; }

        /// <summary>
        /// The current version of the application.
        /// </summary>
        public virtual string Version { get; set; }

        /// <summary>
        /// Maps a config state to the database.
        /// </summary>
        public class CurrentConfigStateMapping : ClassMap<ConfigState>
        {
            /// <summary>
            /// Maps a config state to the database.
            /// </summary>
            public CurrentConfigStateMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Name).Unique();
                Map(x => x.DODSMTPAddress);
                Map(x => x.DeveloperDistroAddress);
                Map(x => x.DeveloperDistroDisplayName);
                Map(x => x.AtwoodGmailAddress);
                Map(x => x.McLeanGmailAddress);
                Map(x => x.DODEmailHost);
                Map(x => x.MusterRolloverTime).CustomType<CustomDBTypes.DBTime>();
                Map(x => x.MusterDueTime).CustomType<CustomDBTypes.DBTime>();
                Map(x => x.IsMusterFinalized);
                Map(x => x.ProfileLockMaxAge);
                Map(x => x.Version);
            }
        }

        #region Startup Method

        /// <summary>
        /// Loads the config from the database.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 98)]
        private static void SetupMuster(CLI.Options.LaunchOptions launchOptions)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var configStates = session.QueryOver<ConfigState>().WhereRestrictionOn(x => x.Name).IsInsensitiveLike(launchOptions.ConfigName).List();

                    if (configStates.Count != 1)
                        throw new Exception("The desired config name, '{0}', resulted in multiple matches ({1}).  Please narrow the name to result in only one match."
                            .FormatS(launchOptions.ConfigName, String.Join(", ", configStates.Select(x => x.Name))));

                    ServiceManagement.ServiceManager.CurrentConfigState = configStates.First();

                    Logging.Log.Info("Config state, '{0}', successfully loaded.".FormatS(ServiceManagement.ServiceManager.CurrentConfigState.Name));

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

    }
}
