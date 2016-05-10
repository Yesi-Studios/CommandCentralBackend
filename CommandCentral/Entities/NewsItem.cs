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
        public virtual List<string> Paragraphs { get; set; }

        /// <summary>
        /// The time this news item was created.
        /// </summary>
        public virtual DateTime CreationTime { get; set; }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a single news item for the given Id or returns null if it does not exist.
        /// <para />
        /// Options: 
        /// <para />
        /// newsitemid - The Id of the news item we want to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void LoadNewsItem_Client(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
            }
            //Get the news item ID we're supposed to load.
            else if (!token.Args.ContainsKey("newsitemid"))
                token.AddErrorMessage("You didn't send an 'newsitemid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
            else
            {
                //Ok, so there is a newsitemid!  Is it legit?
                Guid newsItemId;
                if (!Guid.TryParse(token.Args["newsitemid"] as string, out newsItemId))
                    token.AddErrorMessage("The newsitemid parameter was not in the correct format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                else
                {
                    //Ok, well it's a GUID.   Do we have it in the database?...
                    token.SetResult(token.CommunicationSession.Get<NewsItem>(newsItemId));
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all news items.
        /// <para />
        /// Options: 
        /// <para />
        /// None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void LoadNewsItems_Client(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                //Set the result.
                token.SetResult(token.CommunicationSession.QueryOver<NewsItem>().List());
            }
        }

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates a news item given a title and paragraphs.  Returns the new Id of the news item.
        /// <para />
        /// Options: 
        /// <para />
        /// title - A title for the news item.
        /// paragraphs - The new paragraphs to be added to the new news item.   
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void CreateNewsItem_Client(MessageToken token)
        {

            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                //Let's see if the parameters are here.
                if (!token.Args.ContainsKey("title"))
                    token.AddErrorMessage("You didn't send a 'title' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

                if (!token.Args.ContainsKey("paragraphs"))
                    token.AddErrorMessage("You didn't send a 'paragraphs' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

                if (!token.HasError)
                {
                    string title = token.Args["title"] as string;
                    List<string> paragraphs = token.Args["paragraphs"].CastJToken<List<string>>();


                    //Now build the whole news item.
                    NewsItem newsItem = new NewsItem
                    {
                        CreationTime = token.CallTime,
                        Creator = token.AuthenticationSession.Person,
                        Paragraphs = paragraphs,
                        Title = title
                    };

                    //Now we just need to validate the news item object and throw back any errors if we get them.
                    NewsItemValidator validator = new NewsItemValidator();
                    var results = validator.Validate(newsItem);
                    if (results.IsValid)
                    {
                        //Ok, it's a good news item.  Let's... stick it in.
                        token.CommunicationSession.SaveOrUpdate(newsItem);

                        //Send the Id back to the client.
                        token.SetResult(newsItem.Id);
                    }
                    else
                    {
                        //Send back the error messages.
                        token.AddErrorMessages(results.Errors.Select(x => x.ToString()), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    }
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Updates a news item from the client.
        /// <para />
        /// Options: 
        /// <para />
        /// newsitem - A news item object you want to update.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void UpdateNewsItem_Client(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to update the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                if (!token.AuthenticationSession.Person.PermissionGroups.SelectMany(x => x.SpecialPermissions).ToList().Exists(x => x.Value == "Manage News"))
                {
                    token.AddErrorMessage("You are not authorized to manage the news.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                }
                else
                {
                    //Let's see if the parameters are here.
                    if (!token.Args.ContainsKey("newsitem"))
                        token.AddErrorMessage("You didn't send a 'newsitem' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

                    //Get the news item from the client.
                    NewsItem newsItemFromClient = token.Args["newsitem"].CastJToken<NewsItem>();

                    //Before we even compare it to the database, let's ensure its validity. 
                    NewsItemValidator validator = new NewsItemValidator();
                    var results = validator.Validate(newsItemFromClient);
                    if (results.IsValid)
                    {
                        //Ok, it's a good news item so now we're going to compare it to the one in the database.
                        NewsItem newsItemFromDB = token.CommunicationSession.Get<NewsItem>(newsItemFromClient.Id);

                        if (newsItemFromDB == null)
                        {
                            token.AddErrorMessage("A message token with that Id was not found in the database.", ErrorTypes.Validation, System.Net.HttpStatusCode.NotFound);
                        }
                        else
                        {
                            //Ok, so it's not null and we have what it looks like.  Cool.  Now do the comparisons. 
                            //Then we're going to select out the unauthorized variations.  Those are anything but the title and the paragraphs.
                            var unauthorizedVariances = newsItemFromClient.DetailedCompare(newsItemFromDB).Where(x => x.PropertyName != "title" || x.PropertyName != "paragraphs");

                            if (unauthorizedVariances.Any())
                            {
                                var errors = unauthorizedVariances.Select(x => "You are not authorized to edit the '{0}' property.".FormatS(x.PropertyName));
                                token.AddErrorMessages(errors, ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            }
                            else
                            {
                                //Ok so there's no unauthorized variances.  I guess we can... do the update then?
                                token.CommunicationSession.Update(newsItemFromClient);
                                token.SetResult("Success");
                            }
                        }
                    }
                    else
                    {
                        //Send back the error messages.
                        token.AddErrorMessages(results.Errors.Select(x => x.ToString()), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    }
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Deletes a news item.
        /// <para />
        /// Options: 
        /// <para />
        /// newsitemid - the Id of the news item we want to delete.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void DeleteNewsItem_Client(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to update the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                if (!token.AuthenticationSession.Person.PermissionGroups.SelectMany(x => x.SpecialPermissions).ToList().Exists(x => x.Value == "Manage News"))
                {
                    token.AddErrorMessage("You are not authorized to manage the news.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                }
                else
                {
                    //Get the news item id.
                    if (!token.Args.ContainsKey("newsitemid"))
                        token.AddErrorMessage("You didn't send a 'newsitemid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    else
                    {
                        //Ok, so there is a news item id!  Is it legit?
                        Guid newsItemId;
                        if (!Guid.TryParse(token.Args["newsitemid"] as string, out newsItemId))
                            token.AddErrorMessage("The newsitemid parameter was not in the correct format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        else
                        {
                            //Ok, well it's a GUID.   Do we have it in the database?...  Hope so!
                            var newsItemFromDB = token.CommunicationSession.Get<NewsItem>(newsItemId);

                            if (newsItemFromDB == null)
                            {
                                token.AddErrorMessage("A message token with that Id was not found in the database.", ErrorTypes.Validation, System.Net.HttpStatusCode.NotFound);
                            }
                            else
                            {
                                token.CommunicationSession.Delete(newsItemId);

                                token.SetResult("Success");
                            }
                        }
                    }
                }
            }
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
                        Name = "LoadNewsItem",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None",
                        DataMethod = LoadNewsItem_Client,
                        Description = "Loads a single news item for the given Id or returns null if it does not exist.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                            "newsitemid - The Id of the news item we want to load."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = true
                    },
                    new EndpointDescription
                    {
                        Name = "LoadNewsItems",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None",
                        DataMethod = LoadNewsItems_Client,
                        Description = "Loads all news items.",
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
                    },
                    new EndpointDescription
                    {
                        Name = "DeleteNewsItem",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "Must have the Manage News Permission.",
                        DataMethod = DeleteNewsItem_Client,
                        Description = "Deletes the requested news item.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                            "newsitemid - The Id of the news item you want to delete."
                        },
                        RequiredSpecialPermissions = new[] { "Manage News" }.ToList(),
                        RequiresAuthentication = true
                    },
                    new EndpointDescription
                    {
                        Name = "UpdateNewsItem",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "Must have the Manage News Permission.  Additionally, changes to any property besides the Title or the Paragraphs are not allowed.",
                        DataMethod = DeleteNewsItem_Client,
                        Description = "Updates the requested news item in the database.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                            "newsitem - A properly formatted news item.  This should represent the state of the object as you wish it to be."
                        },
                        RequiredSpecialPermissions = new[] { "Manage News" }.ToList(),
                        RequiresAuthentication = true
                    },
                    new EndpointDescription
                    {
                        Name = "CreateNewsItem",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "Must have the Manage News Permission.",
                        DataMethod = CreateNewsItem_Client,
                        Description = "Creates a news item and returns the item's assigned Id.  The Id, Creator, and CreationTime will be set for you.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                            "title - The new title of the news item.",
                            "paragraphs - a JSON array of strings representing the paragraphs in the news item."
                        },
                        RequiredSpecialPermissions = new[] { "Manage News" }.ToList(),
                        RequiresAuthentication = true
                    }
                };
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
                Table("news_items");

                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator);

                Map(x => x.Title).Not.Nullable().Length(50);
                HasMany(x => x.Paragraphs)
                    .Table("news_item_paragraphs")
                    .KeyColumn("NewsItemID")
                    .Element("Paragraph");
                Map(x => x.CreationTime).Not.Nullable();
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
