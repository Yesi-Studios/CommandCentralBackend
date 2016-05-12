using System;
using System.Collections.Generic;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single muster record, intended to archive the fact that a person claimed that another person was in a given state at a given time.
    /// </summary>
    public class MusterRecord
    {
        #region Properties

        /// <summary>
        /// Unique GUID of this muster record
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Musterer - I hate that word
        /// </summary>
        public virtual Person Musterer { get; set; }

        /// <summary>
        /// The Person being mustered by the musterer, which is the person mustering the person that must be mustered. muster.
        /// </summary>
        public virtual Person Musteree { get; set; }

        /// <summary>
        /// The Person being mustered's rank. Fucking Mustard.
        /// </summary>
        public virtual string Rank { get; set; }

        /// <summary>
        /// The person that is having the muster happen to them's division.
        /// </summary>
        public virtual string Division { get; set; }

        /// <summary>
        /// The individual that is being made accountable for through the process of mustering's department
        /// </summary>
        public virtual string Department { get; set; }

        /// <summary>
        /// The human being chosen to say their name out loud in front of their peers to make sure they are alive and where they should be at that specific time's Command
        /// </summary>
        public virtual string Command { get; set; }

        /// <summary>
        /// The one tiny human being on this planet out of all the other people that signed a contract that has binded him into a life of accountability's muster state.
        /// </summary>
        public virtual string MusterStatus { get; set; }

        /// <summary>
        /// That same person from above's duty status
        /// </summary>
        public virtual string DutyStatus { get; set; }

        /// <summary>
        /// The date and time the person was mustered at.
        /// </summary>
        public virtual DateTime MusterTime { get; set; }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a muster record for a given id and returns null if none exists.
        /// <para />
        /// Options: 
        /// <para />
        /// newsitemid - the Id of the news item we want to delete.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void LoadMusterRecord_Client(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view muster records.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("musterrecordid"))
            {
                token.AddErrorMessage("You must send a muster record id.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid musterRecordId;
            if (!Guid.TryParse(token.Args["musterrecordid"] as string, out musterRecordId))
            {
                token.AddErrorMessage("Your muster record id was not legitsky.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            token.SetResult(token.CommunicationSession.Get<MusterRecord>(musterRecordId));
        }

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
                        Name = "LoadMusterRecord",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None",
                        DataMethod = LoadMusterRecord_Client,
                        Description = "Loads a muster record for a given id and returns null if none exists.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                            "musterrecordid - The Id of the muster record we want to load."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = true
                    }
                };
            }
        }

        #endregion

        /// <summary>
        /// Maps a record to the database.
        /// </summary>
        public class MusterRecordMapping : ClassMap<MusterRecord>
        {
            /// <summary>
            /// Maps a record to the database.
            /// </summary>
            public MusterRecordMapping()
            {
                Table("muster_records");

                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Musterer).Not.Nullable();
                References(x => x.Musteree).Not.Nullable();

                Map(x => x.Rank).Not.Nullable().Length(10);
                Map(x => x.Division).Not.Nullable().Length(10);
                Map(x => x.Department).Not.Nullable().Length(10);
                Map(x => x.Command).Not.Nullable().Length(10);
                Map(x => x.MusterStatus).Not.Nullable().Length(20);
                Map(x => x.DutyStatus).Not.Nullable().Length(20);
                Map(x => x.MusterTime).Not.Nullable();
            }
        }

    }
}
