using System;
using FluentNHibernate.Mapping;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using CommandCentral.ClientAccess;

namespace CommandCentral.Entities
{

    /// <summary>
    /// Describes a single change
    /// </summary>
    public class Change
    {

        #region Properties

        /// <summary>
        /// The Id of this change.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The client who initiated this change.
        /// </summary>
        public virtual Person Editor { get; set; }

        /// <summary>
        /// The name of the object/entity that was changed.
        /// </summary>
        public virtual string ObjectName { get; set; }

        /// <summary>
        /// The Id of the object that was changed.
        /// </summary>
        public virtual Guid ObjectId { get; set; }

        /// <summary>
        /// The variance that caused this change to be logged.
        /// </summary>
        public virtual Variance Variance { get; set; }

        /// <summary>
        /// The time this change was made.
        /// </summary>
        public virtual DateTime Time { get; set; }

        /// <summary>
        /// A free text field describing this change.
        /// </summary>
        public virtual string Remarks { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Inserts a number of changes into the database on parallel threads with no return to the sync context.
        /// </summary>
        /// <param name="changes"></param>
        public static void LogChanges(IEnumerable<Change> changes)
        {
            Task.Factory.StartNew(() =>
                {
                    Parallel.ForEach(changes, change =>
                    {
                        using (var session = DataAccess.NHibernateHelper.CreateSession())
                        using (var transaction = session.BeginTransaction())
                        {
                            try
                            {
                                session.Save(change);

                                transaction.Commit();
                            }
                            catch (Exception e)
                            {
                                //TODO handle errors here since we're in a new thread with no way to get back.
                                transaction.Rollback();
                            }
                        }

                    });
                });
        }

        /// <summary>
        /// Logs the creation of an object.
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="objectName"></param>
        /// <param name="objectId"></param>
        public static void LogCreation(Person editor, string objectName, Guid objectId)
        {
            Task.Factory.StartNew(() =>
            {
                using (var session = DataAccess.NHibernateHelper.CreateSession())
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        session.Save(new Change
                            {
                                Editor = editor,
                                ObjectId = objectId,
                                ObjectName = objectName,
                                Remarks = "Initial Creation",
                                Time = DateTime.Now
                            });

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        //TODO handle errors here since we're in a new thread with no way to get back.
                        transaction.Rollback();
                    }
                }
            });
        }

        /// <summary>
        /// Logs the deletion of an object.
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="objectName"></param>
        /// <param name="objectId"></param>
        public static void LogDeletion(Person editor, string objectName, Guid objectId)
        {
            Task.Factory.StartNew(() =>
            {
                using (var session = DataAccess.NHibernateHelper.CreateSession())
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        session.Save(new Change
                        {
                            Editor = editor,
                            ObjectId = objectId,
                            ObjectName = objectName,
                            Remarks = "Deletion",
                            Time = DateTime.Now
                        });

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        //TODO handle errors here since we're in a new thread with no way to get back.
                        transaction.Rollback();
                    }
                }
            });
        }

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
                        Name = "LoadChanges",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None",
                        DataMethod = LoadChanges_Client,
                        Description = "Loads all changes that have been made to a given object.  Non-returnable fields will have their values redacted.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                            "objectid - the ID of the object for which to load changes."
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
        /// Loads all changes that have been made to a given object.  Non-returnable fields will have their values redacted.
        /// <para />
        /// Options: 
        /// <para />
        /// objectid : the ID of the object for which to load changes.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void LoadChanges_Client(MessageToken token)
        {
            //Make sure we have the parameter we expect and that it is in the Guid format.
            if (!token.Args.ContainsKey("objectid"))
            {
                token.AddErrorMessage("You must send an 'objectid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid objectId;
            if (!Guid.TryParse(token.Args["objectid"] as string, out objectId))
            {
                token.AddErrorMessage("Your object Id was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Get all the changes.
            IList<Change> changes = token.CommunicationSession.QueryOver<Change>().Where(x => x.ObjectId == objectId).List();

            //Now we need to redact all changes the client isn't allowed to see.
            //This will get all returnable fields for the person model.
            var returnableFields = token.AuthenticationSession.Person.PermissionGroups
                                        .SelectMany(x => x.ModelPermissions.Where(y => y.ModelName == DataAccess.NHibernateHelper.GetEntityMetadata("Person").EntityName).SelectMany(y => y.ReturnableFields));

            for (int x = 0; x < changes.Count; x++)
            {
                //If the client doesn't have permission to return this field, then set the values to REDACTED.  Like they some classified shit.
                if (!returnableFields.Contains(changes[x].Variance.PropertyName))
                {
                    changes[x].Variance.NewValue = "REDACTED";
                    changes[x].Variance.OldValue = "REDACTED";
                }
            }

            token.SetResult(changes);
        }

        #endregion

        /// <summary>
        /// Maps a change to the database.
        /// </summary>
        public class ChangeMapping : ClassMap<Change>
        {
            /// <summary>
            /// Maps a change to the database.
            /// </summary>
            public ChangeMapping()
            {
                Table("changes");

                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Editor).Not.Nullable();

                Map(x => x.ObjectName).Not.Nullable().Length(20);
                Map(x => x.ObjectId).Not.Nullable().Length(45);
                Component(x => x.Variance, variance =>
                    {
                        variance.Map(x => x.PropertyName).Nullable();
                        variance.Map(x => x.OldValue).Nullable();
                        variance.Map(x => x.NewValue).Nullable();
                    });
                Map(x => x.Time).Not.Nullable();
                Map(x => x.Remarks).Nullable().Length(150);

            }
        }

    }
}
