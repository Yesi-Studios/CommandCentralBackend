using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Templates
{
    public static class TemplateManager
    {
        public static ConcurrentDictionary<string, string> AllTemplates = new ConcurrentDictionary<string, string>();
    }
}
