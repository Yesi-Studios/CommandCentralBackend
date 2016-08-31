using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CCServ.Logging
{
    /// <summary>
    /// Represents a log message in the database.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// The Id of this log entry.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The log message's actual message.
        /// </summary>
        public virtual string Message { get; set; }

        /// <summary>
        /// The token that represents the interaction during which this message was generated.
        /// </summary>
        public virtual ClientAccess.MessageToken Token { get; set; }

        /// <summary>
        /// The name of the method that generated this log message.
        /// </summary>
        public virtual string CallerMemberName { get; set; }

        /// <summary>
        /// The name of the file in which the member existed.
        /// </summary>
        public virtual string CallerFilePath { get; set; }

        /// <summary>
        /// The line at which this message was logged.
        /// </summary>
        public virtual int CallerLineNumber { get; set; }

        /// <summary>
        /// The message type.
        /// </summary>
        public virtual string MessageType { get; set; }

        /// <summary>
        /// The time this message was generated.
        /// </summary>
        public virtual DateTime Time { get; set; }

        /// <summary>
        /// Creates a log message and sets the Time to now.
        /// </summary>
        public LogEntry()
        {
            Time = DateTime.Now;
        }

        /// <summary>
        /// Maps a log entry to the database.
        /// </summary>
        public class LogEntryMapping : ClassMap<LogEntry>
        {
            /// <summary>
            /// Maps a log entry to the database.
            /// </summary>
            public LogEntryMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Message).Not.Nullable().Length(10000);
                Map(x => x.CallerMemberName);
                Map(x => x.CallerFilePath);
                Map(x => x.CallerLineNumber);
                Map(x => x.MessageType).Not.Nullable();
                Map(x => x.Time);

                References(x => x.Token);
            }
        }
    }
}
