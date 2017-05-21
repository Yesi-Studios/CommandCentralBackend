using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Entities;
using NHibernate.Criterion;

namespace CCServ.ClientAccess.Endpoints
{
    static class CommentEndpoints
    {

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a single comment.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void LoadComment(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            //Get the Id.
            if (!Guid.TryParse(token.Args["id"] as string, out Guid id))
                throw new CommandCentralException("The id parameter was not in the correct format.", ErrorTypes.Validation);

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    //Ok, well it's a GUID.   Do we have it in the database?...
                    var comment = session.Get<Comment>(id);

                    token.SetResult(comment);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates a single comment.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void CreateComment(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("text", "entityownerid");

            var comment = new Comment
            {
                Creator = token.AuthenticationSession.Person,
                Id = Guid.NewGuid(),
                Time = token.CallTime,
                Text = token.Args["text"] as string
            };

            if (!Guid.TryParse(token.Args["entityownerid"] as string, out Guid entityOwnerId))
                throw new CommandCentralException("Your id was not in a valid format.", ErrorTypes.Validation);

            var validationResult = new Comment.CommentValidator().Validate(comment);
            if (!validationResult.IsValid)
            {
                throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));
            }

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var owner = session.Get<ICommentable>(entityOwnerId) ??
                        throw new CommandCentralException("Your entity owner id did not point to an actual, commentable object.", ErrorTypes.Validation);

                    owner.Comments.Add(comment);

                    session.Update(owner);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Updates a single comment.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void UpdateComment(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("comment");

            Comment commentFromClient;
            try
            {
                commentFromClient = token.Args["comment"].CastJToken<Comment>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while parsing your comment.", ErrorTypes.Validation);
            }

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var commentFromDB = session.Get<Comment>(commentFromClient.Id) ??
                        throw new CommandCentralException("Your comment does not exist.  Please consider creating it first.", ErrorTypes.Validation);

                    if (commentFromDB.Creator.Id != token.AuthenticationSession.Person.Id)
                        throw new CommandCentralException("Only the owner of a comment may edit it.", ErrorTypes.Authorization);

                    commentFromDB.Text = commentFromClient.Text;

                    var validationResult = new Comment.CommentValidator().Validate(commentFromDB);
                    if (!validationResult.IsValid)
                    {
                        throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));
                    }

                    session.Update(commentFromDB);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Deletes a single comment.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void DeleteComment(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("comment");

            Comment commentFromClient;
            try
            {
                commentFromClient = token.Args["comment"].CastJToken<Comment>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while parsing your comment.", ErrorTypes.Validation);
            }

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var commentFromDB = session.Get<Comment>(commentFromClient.Id) ??
                        throw new CommandCentralException("Your comment does not exist.  Please consider creating it first.", ErrorTypes.Validation);

                    if (commentFromDB.Creator.Id != token.AuthenticationSession.Person.Id)
                        throw new CommandCentralException("Only the owner of a comment may edit it.", ErrorTypes.Authorization);

                    session.Delete(commentFromDB);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


    }
}
