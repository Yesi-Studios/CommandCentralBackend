using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.ClientAccess;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Defines a single correspondence and maps it to the database.
    /// </summary>
    public class Correspondence
    {
        #region Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Originator of correspondence.
        /// </summary>
        public virtual Person Originator { get; set; }

        /// <summary>
        /// Date and Time correspondence was created.
        /// </summary>
        public virtual DateTime CreatedTime { get; set; }

        /// <summary>
        /// Subject of correspondence.
        /// </summary>
        public virtual string Subject { get; set; }

        /// <summary>
        /// The type of correspondence item (award, eval, etc.)
        /// </summary>
        public virtual string Type { get; set; }

        /// <summary>
        /// Status of correspondence (e.g., routed to N1, returned to department, etc.)
        /// </summary>
        public virtual ReferenceLists.CorrespondenceStatus Status { get; set; }

        #endregion

        #region Client Access 

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Client creates a correspondence item.
        /// <para />
        /// Client Parameters: <para />
        ///     subject - the subject or title of the correspondence item.<para />
        ///     type - the type of correspondence item being created (award, eval, etc.)<para />
        ///     status - status being taken on the correspondence item (routing to n1, cmc, xo, etc.)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "CreateCorrespondence", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_CreateCorrespondence(MessageToken token)
        {

           
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("Authentication failed.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.Args.ContainsKey("subject"))
            {
                token.AddErrorMessage("Misisng subject field.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            string subject = token.Args["subject"] as string;
            if (subject == null)
            {
                token.AddErrorMessage("Subject was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (subject.Length > 50 || !subject.All(char.IsLetterOrDigit))
            {
                token.AddErrorMessage("Invalid characters in your subject.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (!token.Args.ContainsKey("type"))
            {
                token.AddErrorMessage("Missing type field.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            string type = token.Args["type"] as string;
            if (type == null)
            {
                token.AddErrorMessage("Type was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (type.Length > 50 || !type.All(char.IsLetterOrDigit))
            {
                token.AddErrorMessage("Invalid characters in your type.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (!token.Args.ContainsKey("status"))
            {
                token.AddErrorMessage("Missing status field.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            string status = token.Args["status"] as string;
            if (status == null)
            {
                token.AddErrorMessage("Status was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (status.Length > 50 || !status.All(char.IsLetterOrDigit))
            {
                token.AddErrorMessage("Invalid characters in your status.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var corStatus = token.CommunicationSession.QueryOver<ReferenceLists.CorrespondenceStatus>().Where(x => x.Value == status).SingleOrDefault();
            if (corStatus == null)
            {
                token.AddErrorMessage("Invalid status.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Correspondence correspondence = new Correspondence
            {
                Originator = token.AuthenticationSession.Person, 
                Subject = subject,
                CreatedTime = token.CallTime,
                Status = corStatus,
                Type = type
            };

            token.CommunicationSession.Save(correspondence);

            token.SetResult(correspondence.Id);
        }



        #endregion 

        /// <summary>
        /// Maps a correspondence to the database.
        /// </summary>
        public class CorrespondenceMapping : ClassMap<Correspondence> 
        {
            /// <summary>
            /// Maps a correspondence to the database.
            /// </summary>
            public CorrespondenceMapping() 
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Originator).Not.Nullable();
                Map(x => x.CreatedTime).Not.Nullable();
                Map(x => x.Subject).Not.Nullable().Length(50);
                References(x => x.Status).Not.Nullable();
                Map(x => x.Type).Not.Nullable().Length(50);
            }
        }
    }
}
