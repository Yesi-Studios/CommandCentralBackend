using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandDB_Plugin
{
    /// <summary>
    /// Provides members for sending messages to users via text message.
    /// </summary>
    public static class TextMessageHelper
    {
        /// <summary>
        /// Contains mappings between a service provider's name, and their sms smtp address.
        /// </summary>
        public static ConcurrentDictionary<string, string> PhoneCarrierMailDomainMappings = new ConcurrentDictionary<string,string>(new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("Verizon", "@vtext.com")
        }, StringComparer.OrdinalIgnoreCase);
    }
}
