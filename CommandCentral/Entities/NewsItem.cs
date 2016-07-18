using System;
using System.Linq;
using System.Collections.Generic;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using AtwoodUtils;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single News Item and its members, including its DB access members.
    /// </summary>
    public class NewsItem
    {

        #region Properties

        /// <summary>
        /// The Id of the news item.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The client that created the news item.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// The title of the news item.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The paragraphs contained in this news item.
        /// </summary>
        public virtual IList<string> Paragraphs { get; set; }

        /// <summary>
        /// The time this news item was created.
        /// </summary>
        public virtual DateTime CreationTime { get; set; }

        #endregion

        #region Client Access

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
        [EndpointMethod(EndpointName = "LoadNewsItem", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadNewsItem(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Get the news item ID we're supposed to load.
            if (!token.Args.ContainsKey("newsitemid"))
            {
                token.AddErrorMessage("You didn't send an 'newsitemid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Ok, so there is a newsitemid!  Is it legit?
            Guid newsItemId;
            if (!Guid.TryParse(token.Args["newsitemid"] as string, out newsItemId))
            {
                token.AddErrorMessage("The newsitemid parameter was not in the correct format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

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
                        Creator = newsItem.Creator.ToString(),
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
        [EndpointMethod(EndpointName = "LoadNewsItems", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadNewsItems(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Set the result.
                token.SetResult(session.QueryOver<NewsItem>().List().Select(x =>
                    {
                        return new
                        {
                            x.Id,
                            x.CreationTime,
                            Creator = x.Creator.ToString(),
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
        [EndpointMethod(EndpointName = "CreateNewsItem", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_CreateNewsItem(MessageToken token)
        {

            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.ManageNews))
            {
                token.AddErrorMessage("You do not have permission to manage the news.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
            }

            //Let's see if the parameters are here.
            if (!token.Args.ContainsKey("title"))
                token.AddErrorMessage("You didn't send a 'title' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            if (!token.Args.ContainsKey("paragraphs"))
                token.AddErrorMessage("You didn't send a 'paragraphs' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            if (token.HasError)
                return;

            string title = token.Args["title"] as string;
            List<string> paragraphs = null;


            //Do the cast here in case it fails.
            try
            {
                paragraphs = token.Args["paragraphs"].CastJToken<List<string>>();
            }
            catch (Exception e)
            {
                token.AddErrorMessage("There was an error while attempting to cast your parahraphs.  It must be a JSON array of strings.  Error details: {0}".FormatS(e.Message), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
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
            NewsItemValidator validator = new NewsItemValidator();
            var results = validator.Validate(newsItem);

            if (!results.IsValid)
            {
                //Send back the error messages.
                token.AddErrorMessages(results.Errors.Select(x => x.ToString()), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

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
                catch (Exception e)
                {
                    transaction.Rollback();
                    token.AddErrorMessage(e.Message, ErrorTypes.Fatal, System.Net.HttpStatusCode.InternalServerError);
                    return;
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
        [EndpointMethod(EndpointName = "UpdateNewsItem", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_UpdateNewsItem(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.ManageNews))
            {
                token.AddErrorMessage("You do not have permission to manage the news.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
            }

            //Let's see if the parameters are here.
            if (!token.Args.ContainsKey("newsitemid"))
            {
                token.AddErrorMessage("You didn't send a 'newsitemid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Get the news item id from the client.
            Guid newsItemId;
            if (!Guid.TryParse(token.Args["newsitemid"] as string, out newsItemId))
            {
                token.AddErrorMessage("The news item id you sent was not in a valid format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

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
                    NewsItem newsItem = session.Get<NewsItem>(newsItemId);

                    if (newsItem == null)
                    {
                        token.AddErrorMessage("A news item with that Id was not found in the database.", ErrorTypes.Validation, System.Net.HttpStatusCode.NotFound);
                        return;
                    }

                    //Ok, now let's put the values into the news item and then ask if it's valid.
                    if (!string.IsNullOrEmpty(title))
                        newsItem.Title = title;

                    if (paragraphs != null)
                        newsItem.Paragraphs = paragraphs;

                    var errors = new NewsItemValidator().Validate(newsItem).Errors;

                    if (errors.Any())
                    {
                        token.AddErrorMessages(errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok, so it's valid.  Now let's save it.
                    session.Update(newsItem);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    token.AddErrorMessage(e.Message, ErrorTypes.Fatal, System.Net.HttpStatusCode.InternalServerError);
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
        [EndpointMethod(EndpointName = "DeleteNewsItem", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_DeleteNewsItem(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.ManageNews))
            {
                token.AddErrorMessage("You do not have permission to manage the news.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
            }

            //Get the news item id.
            if (!token.Args.ContainsKey("newsitemid"))
            {
                token.AddErrorMessage("You didn't send a 'newsitemid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            
            //Ok, so there is a news item id!  Is it legit?
            Guid newsItemId;
            if (!Guid.TryParse(token.Args["newsitemid"] as string, out newsItemId))
            {
                token.AddErrorMessage("The newsitemid parameter was not in the correct format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok, well it's a GUID.   Do we have it in the database?...  Hope so!
                    var newsItemFromDB = session.Get<NewsItem>(newsItemId);

                    if (newsItemFromDB == null)
                    {
                        token.AddErrorMessage("A message token with that Id was not found in the database.", ErrorTypes.Validation, System.Net.HttpStatusCode.NotFound);
                        return;
                    }

                    session.Delete(newsItemFromDB);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    token.AddErrorMessage(e.Message, ErrorTypes.Fatal, System.Net.HttpStatusCode.InternalServerError);
                    return;
                }
            }
        }

        #endregion

        /// <summary>
        /// Maps a news item to the database.
        /// </summary>
        public class NewsItemMapping : ClassMap<NewsItem>
        {
            /// <summary>
            /// Maps a news item to the database.
            /// </summary>
            public NewsItemMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator).LazyLoad(Laziness.False);

                Map(x => x.Title).Not.Nullable().Length(50).Not.LazyLoad();
                HasMany(x => x.Paragraphs)
                    .KeyColumn("NewsItemID")
                    .Element("Paragraph", x => x.Length(10000))
                    .Not.LazyLoad();
                Map(x => x.CreationTime).Not.Nullable().Not.LazyLoad();
            }
        }

        /// <summary>
        /// Validates the properties of a news item.
        /// </summary>
        public class NewsItemValidator : AbstractValidator<NewsItem>
        {
            /// <summary>
            /// Validates the properties of a news item.
            /// </summary>
            public NewsItemValidator()
            {
                RuleFor(x => x.CreationTime).NotEmpty();
                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Paragraphs)
                    .Must(x => x.Sum(y => y.Length) <= 4096)
                    .WithMessage("The total text in the paragraphs must not exceed 4096 characters.");
                RuleFor(x => x.Title).NotEmpty().Length(3, 50).WithMessage("The title must not be blank and must be between 3 and 50 characters.");
            }
        }
    }
}
