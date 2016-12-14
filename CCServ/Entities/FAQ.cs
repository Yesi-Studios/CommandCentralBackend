using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CCServ.Entities
{
    /// <summary>
    /// A single frequently asked question.
    /// </summary>
    public class FAQ
    {
        #region Properties

        /// <summary>
        /// The unique Guid for this FAQ
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of this FAQ item.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The question's text.
        /// </summary>
        public virtual string Question { get; set; }

        /// <summary>
        /// The answer/text for the FAQ.
        /// </summary>
        public virtual IList<string> Paragraphs { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the question.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Question;
        }

        #endregion

        /// <summary>
        /// Maps an FAQ to the database
        /// </summary>
        public class FAQMapping : ClassMap<FAQ>
        {
            /// <summary>
            /// Maps an FAQ to the database
            /// </summary>
            public FAQMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable();
                Map(x => x.Question).Not.Nullable();
                HasMany(x => x.Paragraphs)
                    .KeyColumn("FAQId")
                    .Element("Paragraph", x => x.Length(1000));
            }
        }
    }
}
