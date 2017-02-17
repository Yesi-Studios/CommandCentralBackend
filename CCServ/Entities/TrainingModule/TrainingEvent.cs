using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.TrainingModule
{
    /// <summary>
    /// Describes a single training event.  Events are meant to be scheduled things that can meet a number of requirements for anyone who has it assigned.
    /// </summary>
    public class TrainingEvent
    {

        #region Properties

        /// <summary>
        /// The unique Id of this Training event.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The time that this event will occur.
        /// </summary>
        public virtual DateTime TimeOfEvent { get; set; }

        /// <summary>
        /// The title of this event.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The description of this event.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The person who created this event.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// The date that this event was created.
        /// </summary>
        public virtual DateTime DateCreated { get; set; }

        /// <summary>
        /// The list of people who are facilitating this training.
        /// </summary>
        public virtual IList<Person> Facilitators { get; set; }

        /// <summary>
        /// The list of those people who attended this event.
        /// </summary>
        public virtual IList<Person> Attendees { get; set; }

        /// <summary>
        /// The list of those requirements that this training event satisfies for all attendees.
        /// </summary>
        public virtual IList<Requirement> RequirementsSatisfied { get; set; }

        /// <summary>
        /// Indicates, if true, that this training event is no longer accepting attendees to be added to it.
        /// </summary>
        public virtual bool IsClosed { get; set; }

        /// <summary>
        /// If IsClosed is set to true, this should have the date/time that the training event was closed.
        /// </summary>
        public virtual DateTime DateClosed { get; set; }

        /// <summary>
        /// The list of comments for this event.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; }

        #endregion

        /// <summary>
        /// Maps a training event to the database.
        /// </summary>
        public class TrainingEventMapping : ClassMap<TrainingEvent>
        {
            /// <summary>
            /// Maps a training event to the database.
            /// </summary>
            public TrainingEventMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator).Not.Nullable();

                HasManyToMany(x => x.Facilitators)
                    .Table("persontotrainingevent_facilitators");
                HasManyToMany(x => x.Attendees)
                    .Table("persontotrainingevent_attendees");
                HasManyToMany(x => x.RequirementsSatisfied)
                    .Table("requirementtotrainingevent_requirementssatisfied");

                HasMany(x => x.Comments);

                Map(x => x.DateClosed).Nullable();
                Map(x => x.IsClosed).Default(false.ToString());
                Map(x => x.DateCreated).Not.Nullable();
                Map(x => x.Title).Not.Nullable();
                Map(x => x.Description).Not.Nullable().Length(500);
            }
        }

    }
}
