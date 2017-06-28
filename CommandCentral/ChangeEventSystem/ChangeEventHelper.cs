using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.Entities;
using NHibernate.Linq;
using AtwoodUtils;
using System.Reflection;

namespace CommandCentral.ChangeEventSystem
{
    /// <summary>
    /// A number of members to assist in dealing with change events.
    /// </summary>
    public static class ChangeEventHelper
    {
        private static List<IChangeEvent> _allChangeEvents;

        /// <summary>
        /// Returns all the changed events that inherit from IChangeEvent and caches the results for the next call.
        /// </summary>
        public static List<IChangeEvent> AllChangeEvents
        {
            get
            {
                if (_allChangeEvents != null)
                    return _allChangeEvents;

                _allChangeEvents = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(x => typeof(IChangeEvent).IsAssignableFrom(x) && x != typeof(IChangeEvent))
                    .Select(type =>
                    {
                        return (IChangeEvent)Activator.CreateInstance(type);
                    })
                    .ToList();

                return _allChangeEvents;
            }
        }

        /// <summary>
        /// Given a person, determines all those people who can listen to this event.
        /// </summary>
        /// <param name="person">This is the person to whom the change occurred. If null, it means a system event was raised NOT regarding a specific person.</param>
        /// <param name="changeEvent"></param>
        /// <returns></returns>
        public static IEnumerable<System.Net.Mail.MailAddress> GetValidSubscriptionEmailAddresses(Person person, IChangeEvent changeEvent)
        {
            if (changeEvent.RestrictToChainOfCommand && person == null)
                throw new Exception("If a change event requires a chain of command check, then the person argument can't be null.");

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                //First, let's build the basic query for a person with this event in their subscriptions.
                var query = session.Query<Person>().Where(x => x.SubscribedEvents.ContainsKey(changeEvent.Id));

                //If this event requires a CoC check, then let's use get chain of command to get all those persons' ids who are in this person's chain of command.
                if (changeEvent.RestrictToChainOfCommand)
                {
                    var chainOfCommand = person.GetChainOfCommand();
                    var personIds = chainOfCommand[Authorization.ChainsOfCommand.Main].SelectMany(x => x.Value.Select(y => y.Id)).ToList();

                    query = query.Where(x => personIds.Contains(x.Id));
                }

                //Execute that query.
                var subscribers = query.Fetch(x => x.EmailAddresses).ToList();

                //Make sure the person isn't null before we ask questions about chain of command.
                if (person != null)
                {
                    //Now, we need to select out only those people whose subscription levels match.
                    //So, if the person subscribed at the division level, but the person in question is only common at the department level, throw out the subscribers.
                    subscribers = subscribers.Where(subscriber =>
                    {
                        //Here we're going to get the first event whose name matches this event and of those, the highest level.
                        var subscriptionEvent = subscriber.SubscribedEvents.FirstOrDefault(y => y.Key == changeEvent.Id &&
                            (y.Value == ChainOfCommandLevels.Command ||
                             y.Value == ChainOfCommandLevels.Department ||
                             y.Value == ChainOfCommandLevels.Division));

                        //Ok now that we have that, we're going to ask about the levels and about the subscriber's level.
                        if (subscriptionEvent.Value == ChainOfCommandLevels.Command)
                            return subscriber.IsInSameCommandAs(person);

                        if (subscriptionEvent.Value == ChainOfCommandLevels.Department)
                            return subscriber.IsInSameDepartmentAs(person);

                        if (subscriptionEvent.Value == ChainOfCommandLevels.Division)
                            return subscriber.IsInSameDivisionAs(person);

                        throw new Exception("While processing the change event, '{0}', we found a subscription to that event with an invalid level: '{1}'.".With(changeEvent.Id, subscriptionEvent.Value));
                    }).ToList();
                }

                //Here are all the email addresses of all the people.
                return subscribers.SelectMany(x => x.EmailAddresses.Where(y => y.IsPreferred)
                                                    .Select(y => new System.Net.Mail.MailAddress(y.Address, x.ToString())));
            }
        }
    }
}
