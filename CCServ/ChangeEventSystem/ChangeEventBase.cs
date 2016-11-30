using CCServ.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using NHibernate.Criterion;

namespace CCServ.ChangeEventSystem
{
    public abstract class ChangeEventBase
    {

        #region Properties

        public string Name { protected set; get; }

        public string Description { protected set; get; }

        public bool RequiresChainOfCommand { protected set; get; }

        public Dictionary<string, Dictionary<string, List<string>>> RequiredFields { protected set; get; }

        public string EventRaisedByFriendlyName { get; set; }

        #endregion

        /// <summary>
        /// Prototype for the raise event.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="client"></param>
        public abstract void RaiseEvent(object state, Person client);

        /// <summary>
        /// Given a person, determines all those people who can listen to this event.
        /// </summary>
        /// <param name="person">This is the person to whom the change occurred. If null, it means a system event was raised NOT regarding a specific person.</param>
        /// <returns></returns>
        public List<System.Net.Mail.MailAddress> GetValidSubscriptionEmailAddresses(Person person)
        {
            List<System.Net.Mail.MailAddress> result;

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //First, let's build the basic query for a person with this event in their subscriptions.
                    ChangeEventSubscription subAlias = null;
                    var query = session.QueryOver<Person>().JoinAlias(x => x.SubscribedEvents, () => subAlias)
                        .Where(Restrictions.On(() => subAlias.ChangeEventName).IsInsensitiveLike(this.Name, MatchMode.Anywhere));

                    //If this event requires a CoC check, then let's use get chain of command to get all those persons' ids who are in this person's chain of command.
                    if (this.RequiresChainOfCommand)
                    {

                        if (person == null)
                            throw new ArgumentException("If a change event requires a chain of command check, then the person argument can't be null.");

                        var chainOfCommand = person.GetChainOfCommand();
                        var personIds = chainOfCommand[Authorization.ChainsOfCommand.Main].SelectMany(x => x.Value.Select(y => y.Id.ToString()));

                        query = query.WhereRestrictionOn(x => x.Id).IsInG(personIds);
                    }

                    //Execute that query.
                    var subscribers = query.Fetch(x => x.SubscribedEvents).Eager
                                           .Fetch(x => x.EmailAddresses).Eager
                                           .List<Person>();

                    //Make sure the perso isn't null before we ask questions about chain of command.
                    if (person != null)
                    {
                        //Now, we need to select out only those people whose subscription levels match.
                        //So, if the person subscribed at the division level, but the person in question is only common at the department level, throw out the subscibers.
                        subscribers = subscribers.Where(subscriber =>
                            {
                                //Here we're going to get the first event whose name matches this event and of those, the highest level.
                                var subscriptionEvent = subscriber.SubscribedEvents.FirstOrDefault(y => y.ChangeEventName.SafeEquals(this.Name) &&
                                    (y.ChainOfCommandLevel == ChainOfCommandLevels.Command ||
                                     y.ChainOfCommandLevel == ChainOfCommandLevels.Department ||
                                     y.ChainOfCommandLevel == ChainOfCommandLevels.Division));

                                //Ok now that we have that, we're going to ask about the levels and about the subscriber's level.
                                if (subscriptionEvent.ChainOfCommandLevel == ChainOfCommandLevels.Command)
                                    return subscriber.IsInSameCommandAs(person);

                                if (subscriptionEvent.ChainOfCommandLevel == ChainOfCommandLevels.Department)
                                    return subscriber.IsInSameDepartmentAs(person);

                                if (subscriptionEvent.ChainOfCommandLevel == ChainOfCommandLevels.Division)
                                    return subscriber.IsInSameDivisionAs(person);

                                throw new Exception("Whole processing the change event, '{0}', we found a subscription to that event with an invalid level: '{1}'.".FormatS(this.Name, subscriptionEvent.ChainOfCommandLevel));
                            }).ToList();
                    }

                    //Here are all the email addresses of all the people.
                    result = subscribers.SelectMany(x => x.EmailAddresses.Where(y => y.IsPreferred)
                                                        .Select(y => new System.Net.Mail.MailAddress(y.Address, x.ToString())))
                                             .ToList();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return result;
        }
    }
}
