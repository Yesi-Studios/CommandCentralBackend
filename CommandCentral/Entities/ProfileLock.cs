using System;
using FluentNHibernate.Mapping;
using CommandCentral.ClientAccess;
using System.Collections.Generic;
using AtwoodUtils;
using System.Linq;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single profile lock.
    /// </summary>
    public class ProfileLock
    {

        #region Properties

        /// <summary>
        /// The unique GUID assigned to this Profile Lock
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The person who owns this lock.
        /// </summary>
        public virtual Person Owner { get; set; }

        /// <summary>
        /// The Person whose profile is locked.
        /// </summary>
        public virtual Person LockedPerson { get; set; }

        /// <summary>
        /// The time at which this lock was submitted.
        /// </summary>
        public virtual DateTime SubmitTime { get; set; }

        #endregion

        #region Client Access

        /// <summary>
        /// The endpoints
        /// </summary>
        public static List<EndpointDescription> EndpointDescriptions
        {
            get
            {
                return new List<EndpointDescription>
                {
                    new EndpointDescription
                    {
                        Name = "GetProfileLockByOwner",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None",
                        DataMethod = GetProfileLockByOwner_Client,
                        Description = "Given a person ID, gets a profile lock owned by that person or null if none exists.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = true
                    }
                };
            }
        }

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person ID, gets a profile lock owned by that person or null if none exists.
        /// <para />
        /// Options: 
        /// <para />
        /// personid : the person for whom to check owns a profile lock.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void GetProfileLockByOwner_Client(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You need to be logged in to request profile locks.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("personid"))
            {
                token.AddErrorMessage("You didn't send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("The 'personid' parameter", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            token.SetResult(token.CommunicationSession.QueryOver<ProfileLock>().Where(x => x.Owner.Id == personId).SingleOrDefault());
        }

        private static void GetProfileLockByLockedPerson_Client(MessageToken token)
        {


        }

        #endregion Client Access

        /// <summary>
        /// Maps a profile lock to the database.
        /// </summary>
        public class ProfileLockMapping : ClassMap<ProfileLock>
        {
            /// <summary>
            /// Maps a profile lock to the database.
            /// </summary>
            public ProfileLockMapping()
            {
                Table("profile_locks");

                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Owner).Not.Nullable();
                References(x => x.LockedPerson).Not.Nullable();

                Map(x => x.SubmitTime).Not.Nullable();

                Cache.ReadWrite();
            }
        }
    }
}
