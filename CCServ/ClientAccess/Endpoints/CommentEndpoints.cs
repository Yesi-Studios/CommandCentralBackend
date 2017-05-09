using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Entities;
using Humanizer;
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
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void LoadComment(MessageToken token, DTOs.CommentEndpoints.LoadComment dto)
        {
            token.AssertLoggedIn();

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    //Ok, well it's a GUID.   Do we have it in the database?...
                    var comment = session.Get<Comment>(dto.Id);

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
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void CreateComment(MessageToken token, DTOs.CommentEndpoints.CreateComment dto)
        {
            token.AssertLoggedIn();

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var owner = session.Get<ICommentable>(dto.EntityOwnerId) ??
                        throw new CommandCentralException("Your entity owner id did not point to an actual, commentable object.", ErrorTypes.Validation);

                    owner.Comments.Add(new Comment
                    {
                        Creator = token.AuthenticationSession.Person,
                        Id = Guid.NewGuid(),
                        Time = token.CallTime,
                        Text = dto.Text
                    });

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
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void UpdateComment(MessageToken token, DTOs.CommentEndpoints.UpdateComment dto)
        {
            token.AssertLoggedIn();

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var commentFromDB = session.Get<Comment>(dto.Id) ??
                        throw new CommandCentralException("Your comment does not exist.  Please consider creating it first.", ErrorTypes.Validation);

                    if (commentFromDB.Creator.Id != token.AuthenticationSession.Person.Id)
                        throw new CommandCentralException("Only the owner of a comment may edit it.", ErrorTypes.Authorization);

                    commentFromDB.Text = dto.Text;

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
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void DeleteComment(MessageToken token, DTOs.CommentEndpoints.DeleteComment dto)
        {
            token.AssertLoggedIn();

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var commentFromDB = session.Get<Comment>(dto.Id) ??
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
