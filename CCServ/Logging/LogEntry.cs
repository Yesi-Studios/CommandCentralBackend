using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CCServ.Logging
{
    public class LogEntry
    {
        public virtual Guid Id { get; set; }

        public virtual string Message { get; set; }

        public virtual ClientAccess.MessageToken Token { get; set; }

        public virtual string CallerMemberName { get; set; }

        public virtual string CallerFilePath { get; set; }

        public virtual int CallerLineNumber { get; set; }

        public virtual string MessageType { get; set; }

        public virtual DateTime Time { get; set; }

        public LogEntry()
        {
            if (Id == default(Guid))
                Id = Guid.NewGuid();

            Time = DateTime.Now;
        }

        public class LogEntryMapping : ClassMap<LogEntry>
        {
            public LogEntryMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

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
