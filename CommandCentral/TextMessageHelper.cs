using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CommandCentral
{
    /// <summary>
    /// Provides members for sending messages to users via text message.
    /// </summary>
    public static class TextMessageHelper
    {
        /// <summary>
        /// Contains mappings between a service provider's name, and their sms smtp address.
        /// </summary>
        public static ConcurrentDictionary<string, string> PhoneCarrierMailDomainMappings = new ConcurrentDictionary<string,string>(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Verizon", "@vtext.com")
        }, StringComparer.OrdinalIgnoreCase);
    }
}
