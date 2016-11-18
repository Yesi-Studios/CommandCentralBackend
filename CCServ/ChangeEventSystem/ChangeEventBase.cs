using CCServ.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.ChangeEventSystem
{
    public abstract class ChangeEventBase
    {

        #region Properties

        public string Name { protected set; get; }

        public string Description { protected set; get; }

        public bool RequiresChainOfCommand { protected set; get; }

        public Dictionary<string, Dictionary<string, List<string>>> RequiredFields { protected set; get; }

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
        /// <param name="client"></param>
        /// <returns></returns>
        public List<System.Net.Mail.MailAddress> GetValidSubscriptionEmailAddresses(Person client)
        {
            List<System.Net.Mail.MailAddress> result;

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    NHibernate.IQuery query;

                    //First, let's build the basic query for a person with this event in their subscriptions.
                    string queryString = "from Person as person where '{0}' in elements(person.SubscribedEvents)".FormatS(Name);

                    //If this event requires a CoC check, then let's use get chain of command to get all those persons' ids who are in this person's chain of command.
                    if (RequiresChainOfCommand)
                    {

                        if (client == null)
                            throw new ArgumentException("If a change event requires a chain of command check, then the client can't be null.");

                        var chainOfCommand = client.GetChainOfCommand();
                        var personIds = chainOfCommand[Authorization.ChainsOfCommand.Main].SelectMany(x => x.Value.Select(y => y.Id.ToString()));

                        queryString += " and person.Id in elements(:personIds)";

                        query = session.CreateQuery(queryString)
                            .SetParameter("personIds", personIds);
                    }
                    else
                    {
                        query = session.CreateQuery(queryString);
                    }

                    //Execute that query.
                    var persons = query.List<Person>();

                    //Here are all the email addresses of all the people.
                    result = persons.SelectMany(x =>
                                            x.EmailAddresses.Where(y => y.IsPreferred)
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
