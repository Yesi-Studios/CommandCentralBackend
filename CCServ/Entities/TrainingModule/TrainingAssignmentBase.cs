﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.TrainingModule
{
    /// <summary>
    /// The base elements of a training assignment.
    /// </summary>
    public abstract class TrainingAssignmentBase
    {

        #region Properties

        /// <summary>
        /// Unique Id of this assignment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The date/time that this assignment was created.
        /// </summary>
        public virtual DateTime DateAssigned { get; set; }

        /// <summary>
        /// The person who assigned this assignment.
        /// </summary>
        public virtual Person AssignedBy { get; set; }

        /// <summary>
        /// The person who this assignment is assigned to.  This could be considered the owner.
        /// </summary>
        public virtual Person AssignedTo { get; set; }

        /// <summary>
        /// The date by which this assignment should be completed.
        /// </summary>
        public virtual DateTime CompleteByDate { get; set; }

        /// <summary>
        /// The comments tied to this instance of a training assignment.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; }

        #endregion

        /// <summary>
        /// Maps the base training assignment to the database.
        /// </summary>
        public class TrainingAssignmentBaseMapping : ClassMap<TrainingAssignmentBase>
        {
            /// <summary>
            /// Maps a requirement assignment to the database.
            /// </summary>
            public TrainingAssignmentBaseMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.AssignedBy).Not.Nullable();
                References(x => x.AssignedTo).Not.Nullable();

                Map(x => x.DateAssigned).Not.Nullable();
                Map(x => x.CompleteByDate).Not.Nullable();

                HasMany(x => x.Comments);

                DiscriminateSubClassesOnColumn("Type");
            }
        }

    }
}
