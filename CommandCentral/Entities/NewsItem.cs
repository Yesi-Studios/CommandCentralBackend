using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;
using CommandCentral.ClientAccess;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single News Item and its members, including its DB access members.
    /// </summary>
    public class NewsItem
    {

        #region Properties

        /// <summary>
        /// The ID of the news item.
        /// </summary>
        public virtual string ID { get; set; }

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

        /// <summary>
        /// The endpoints
        /// </summary>
        public static Dictionary<string, EndpointDescription> EndpointDescriptions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

                Id(x => x.ID).GeneratedBy.Guid();

                References(x => x.Creator);

                Map(x => x.Title).Not.Nullable().Length(50);
                HasMany(x => x.Paragraphs)
                    .Table("news_item_paragraphs")
                    .KeyColumn("NewsItemID")
                    .Element("Paragraph");
                Map(x => x.CreationTime).Not.Nullable();
            }
        }
    }
}
