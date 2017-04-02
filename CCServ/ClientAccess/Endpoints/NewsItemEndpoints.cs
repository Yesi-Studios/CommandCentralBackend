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
        /// <para />
        /// Client Parameters: <para />
        ///     newsitemid - The Id of the news item we want to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadNewsItem(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("newsitemid");

            //Ok, so there is a newsitemid!  Is it legit?
            if (!Guid.TryParse(token.Args["newsitemid"] as string, out Guid newsItemId))
                throw new CommandCentralException("The newsitemid parameter was not in the correct format.", HttpStatusCodes.BadRequest);

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Ok, well it's a GUID.   Do we have it in the database?...
                var newsItem = session.Get<NewsItem>(newsItemId);

                //If we have no news item, then give them null.
                if (newsItemId == null)
                {
                    token.SetResult(null);
                    return;
                }

                //If we got a news item, then we need a DTO.
                token.SetResult(new
                {
                    newsItem.Id,
                    newsItem.CreationTime,
                    Creator = newsItem.Creator,
                    newsItem.Paragraphs,
                    newsItem.Title
                });
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all news items.
        /// <para />
        /// Client Parameters: <para />
        /// None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadNewsItems(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var query = session.QueryOver<NewsItem>();

                if (token.Args.ContainsKey("limit"))
                {
                    var limit = Convert.ToInt32(token.Args["limit"]);

                    if (limit <= 0)
                        throw new CommandCentralException("Your limit must be greater than zero.", HttpStatusCodes.BadRequest);

                    query = (NHibernate.IQueryOver<NewsItem, NewsItem>)query.OrderBy(x => x.CreationTime).Desc.Take(limit);
                }

                //Set the result.
                token.SetResult(query.List().Select(x =>
                {
                    return new
                    {
                        x.Id,
                        x.CreationTime,
                        Creator = x.Creator,
                        x.Paragraphs,
                        x.Title
                    };
                }));
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates a news item given a title and paragraphs.  Returns the new Id of the news item.
        /// <para />
        /// Client Parameters: <para />
        ///     title - A title for the news item. <para />
        ///     paragraphs - The new paragraphs to be added to the new news item.   
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateNewsItem(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("title", "paragraphs");

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditNews.ToString()))
                throw new CommandCentralException("You do not have permission to manage the news.", HttpStatusCodes.Unauthorized);

            string title = token.Args["title"] as string;
            List<string> paragraphs = null;

            //Do the cast here in case it fails.
            try
            {
                paragraphs = token.Args["paragraphs"].CastJToken<List<string>>();
            }
            catch (Exception e)
            {
                throw new CommandCentralException("There was an error while attempting to cast your parahraphs.  " +
                    "It must be a JSON array of strings.  Error details: {0}".FormatS(e.Message), HttpStatusCodes.BadRequest);
            }

            //Now build the whole news item.
            NewsItem newsItem = new NewsItem
            {
                Id = Guid.NewGuid(),
                CreationTime = token.CallTime,
                Creator = token.AuthenticationSession.Person,
                Paragraphs = paragraphs,
                Title = title
            };

            //Now we just need to validate the news item object and throw back any errors if we get them.
            var results = new NewsItem.NewsItemValidator().Validate(newsItem);

            if (!results.IsValid)
                throw new AggregateException(results.Errors.Select(x => new CommandCentralException(x.ToString(), HttpStatusCodes.BadRequest)));

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok, it's a good news item.  Let's... stick it in.
                    session.Save(newsItem);

                    //Send the Id back to the client.
                    token.SetResult(newsItem.Id);

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
        /// <para />
        /// Client Parameters: <para />
        ///     newsitem - A news item object you want to update.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdateNewsItem(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("newsitemid");

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditNews.ToString()))
            {
                throw new CommandCentralException("You do not have permission to manage the news.", HttpStatusCodes.Unauthorized);
            }

            //Get the news item id from the client.
            if (!Guid.TryParse(token.Args["newsitemid"] as string, out Guid newsItemId))
                throw new CommandCentralException("The news item id you sent was not in a valid format.", HttpStatusCodes.BadRequest);

            //Before we go get the news item from the database, let's get the title and the paragraphs from the client.  Both are optional.
            string title = null;
            if (token.Args.ContainsKey("title"))
                title = token.Args["title"] as string;

            List<string> paragraphs = null;
            if (token.Args.ContainsKey("paragraphs"))
                paragraphs = token.Args["paragraphs"].CastJToken<List<string>>();

            //Now we can go load the news item.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok, it's a good news item so now we're going to compare it to the one in the database.
                    NewsItem newsItem = session.Get<NewsItem>(newsItemId) ??
                        throw new CommandCentralException("A news item with that Id was not found in the database.", HttpStatusCodes.BadRequest);

                    //Ok, now let's put the values into the news item and then ask if it's valid.
                    if (!string.IsNullOrEmpty(title))
                        newsItem.Title = title;

                    if (paragraphs != null)
                        newsItem.Paragraphs = paragraphs;

                    var validationResult = new NewsItem.NewsItemValidator().Validate(newsItem);

                    if (!validationResult.IsValid)
                        throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));
                    
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
        /// <para />
        /// Client Parameters: <para />
        ///     newsitemid - the Id of the news item we want to delete.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteNewsItem(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("newsitemid");

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditNews.ToString()))
            {
                throw new CommandCentralException("You do not have permission to manage the news.", HttpStatusCodes.Unauthorized);
            }

            //Get the news item id from the client.
            if (!Guid.TryParse(token.Args["newsitemid"] as string, out Guid newsItemId))
                throw new CommandCentralException("The news item id you sent was not in a valid format.", HttpStatusCodes.BadRequest);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok, well it's a GUID.   Do we have it in the database?...  Hope so!
                    var newsItemFromDB = session.Get<NewsItem>(newsItemId) ??
                        throw new CommandCentralException("A message token with that Id was not found in the database.", HttpStatusCodes.BadRequest);

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
