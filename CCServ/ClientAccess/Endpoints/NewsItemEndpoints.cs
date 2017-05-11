using CCServ.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;

namespace CCServ.ClientAccess.Endpoints
{
    static class NewsItemEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a single news item for the given Id or returns null if it does not exist.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadNewsItem(MessageToken token, DTOs.NewsItemEndpoints.LoadNewsItem dto)
        {
            token.AssertLoggedIn();

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                token.SetResult(session.Get<NewsItem>(dto.Id));
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all news items.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadNewsItems(MessageToken token, DTOs.NewsItemEndpoints.LoadNewsItems dto)
        {
            token.AssertLoggedIn();

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var query = session.QueryOver<NewsItem>();

                if (dto.Limit.HasValue)
                {
                    query = (NHibernate.IQueryOver<NewsItem, NewsItem>)query.OrderBy(x => x.CreationTime).Desc.Take(dto.Limit.Value);
                }

                //Set the result.
                token.SetResult(query.List());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates a news item given a title and paragraphs.  Returns the new Id of the news item.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateNewsItem(MessageToken token, DTOs.NewsItemEndpoints.CreateNewsItem dto)
        {
            token.AssertLoggedIn();

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditNews.ToString()))
                throw new CommandCentralException("You do not have permission to manage the news.", ErrorTypes.Authorization);

            //Now build the whole news item.
            NewsItem newsItem = new NewsItem
            {
                Id = Guid.NewGuid(),
                CreationTime = token.CallTime,
                Creator = token.AuthenticationSession.Person,
                Paragraphs = dto.Paragraphs,
                Title = dto.Title
            };

            //Now we just need to validate the news item object and throw back any errors if we get them.
            var results = new NewsItem.NewsItemValidator().Validate(newsItem);

            if (!results.IsValid)
                throw new AggregateException(results.Errors.Select(x => new CommandCentralException(x.ToString(), ErrorTypes.Validation)));

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok, it's a good news item.  Let's... stick it in.
                    session.Save(newsItem);

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
        /// Updates a news item from the client.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdateNewsItem(MessageToken token, DTOs.NewsItemEndpoints.UpdateNewsItem dto)
        {
            token.AssertLoggedIn();

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditNews.ToString()))
            {
                throw new CommandCentralException("You do not have permission to manage the news.", ErrorTypes.Authorization);
            }

            //Now we can go load the news item.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok, it's a good news item so now we're going to compare it to the one in the database.
                    NewsItem newsItem = session.Get<NewsItem>(dto.Id) ??
                        throw new CommandCentralException("A news item with that Id was not found in the database.", ErrorTypes.Validation);

                    //Ok, now let's put the values into the news item and then ask if it's valid.
                    newsItem.Title = dto.Title;
                    newsItem.Paragraphs = dto.Paragraphs;

                    var validationResult = new NewsItem.NewsItemValidator().Validate(newsItem);

                    if (!validationResult.IsValid)
                        throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));
                    
                    //Ok, so it's valid.  Now let's save it.
                    session.Update(newsItem);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return;
                }
            }

        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Deletes a news item.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteNewsItem(MessageToken token, DTOs.NewsItemEndpoints.DeleteNewsItem dto)
        {
            token.AssertLoggedIn();

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditNews.ToString()))
            {
                throw new CommandCentralException("You do not have permission to manage the news.", ErrorTypes.Authorization);
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok, well it's a GUID.   Do we have it in the database?...  Hope so!
                    var newsItemFromDB = session.Get<NewsItem>(dto.Id) ??
                        throw new CommandCentralException("A news item with that Id was not found in the database.", ErrorTypes.Validation);

                    session.Delete(newsItemFromDB);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return;
                }
            }
        }
    }
}
