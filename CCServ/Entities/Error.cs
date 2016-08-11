using System;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using CCServ.Authorization;

namespace CCServ.Entities
{
    /// <summary>
    /// Describes a single error.
    /// </summary>
    public class Error
    {
        #region Properties

        /// <summary>
        /// The unique Id assigned to this error
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The message that was raised for this error
        /// </summary>
        public virtual string Message { get; set; }

        /// <summary>
        /// The stack trace that lead to this error
        /// </summary>
        public virtual string StackTrace { get; set; }

        /// <summary>
        /// Any inner exception's message
        /// </summary>
        public virtual string InnerException { get; set; }

        /// <summary>
        /// The target site of the error.
        /// </summary>
        public virtual string TargetSite { get; set; }

        /// <summary>
        /// The message token representing the request during which the error occurred.
        /// </summary>
        public virtual MessageToken Token { get; set; }

        /// <summary>
        /// The Date/Time this error occurred.
        /// </summary>
        public virtual DateTime Time { get; set; }

        /// <summary>
        /// Indicates whether or not the development/maintenance team has dealt with what caused this error.
        /// </summary>
        public virtual bool IsHandled { get; set; }

        /// <summary>
        /// A comment attached to the error.
        /// </summary>
        public virtual string Comment { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new error given an exception and details about the request in which the error occurred.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="dateTime"></param>
        /// <param name="token"></param>
        public Error(Exception e, DateTime dateTime, MessageToken token)
        {
            this.Message = e.Message;
            this.StackTrace = e.StackTrace;
            this.InnerException = e.StackTrace;
            this.TargetSite = e.TargetSite.Name;
            this.Time = dateTime;
            this.IsHandled = false;
            this.Token = token;
        }

        /// <summary>
        /// This empty constructor is only here for NHibernate to use.  Please use the other one when making an error explicitly.
        /// </summary>
        public Error()
        {
        }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all errors if the client is a developer.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadErrors", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadErrors(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to load errors.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Is the client a developer?
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("Only developers are allowed to view errors.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            //Now we know that we're allowed to send errors.  Let's do that.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                token.SetResult(session.QueryOver<Error>().List());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Attempts to update the ishandled and the comment property for the given error.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "UpdateError", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_UpdateError(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to edit errors.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Is the client a developer?
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("Only developers are allowed to edit errors.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            //Now we need the error the client wants to update.
            if (!token.Args.ContainsKey("errorid"))
            {
                token.AddErrorMessage("You failed to send an 'errorid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Is it a guid?
            Guid errorId;
            if (!Guid.TryParse(token.Args["errorid"] as string, out errorId))
            {
                token.AddErrorMessage("The 'errorid' parameter you sent was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now try to get the error.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var error = session.Get<Error>(errorId);

                    if (error == null)
                    {
                        token.AddErrorMessage("The error id you sent did not correspond to a real error.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //if the client sent us a is handled then let's validate it and set it.
                    if (token.Args.ContainsKey("ishandled"))
                    {
                        bool isHandled;
                        if (!Boolean.TryParse(token.Args["ishandled"] as string, out isHandled))
                        {
                            token.AddErrorMessage("Your 'ishandled' parameter could not be cast to a boolean.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }
                        else
                        {
                            error.IsHandled = isHandled;
                        }
                    }

                    //if the client sent a comment, set it.
                    if (token.Args.ContainsKey("comment"))
                    {
                        error.Comment = token.Args["comment"] as string;
                    }

                    session.Merge(error);

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
        /// Maps an error to the database
        /// </summary>
        public class ErrorMapping : ClassMap<Error>
        {
            /// <summary>
            /// Maps an error to the database
            /// </summary>
            public ErrorMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Message).Not.Nullable().Length(10000);
                Map(x => x.StackTrace).Not.Nullable().Length(10000);
                Map(x => x.InnerException).Not.Nullable().Length(10000);
                Map(x => x.TargetSite).Not.Nullable().Length(10000);
                Map(x => x.Comment).Nullable().Length(10000);
                Map(x => x.Time).Not.Nullable();
                Map(x => x.IsHandled).Not.Nullable();

                References(x => x.Token).LazyLoad(Laziness.False);
            }
        }

    }
}
