using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    public class ConnectionSettings
    {

        public string Database { get; set; }
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<string> SMTPHosts { get; set; }
        public bool RebuildIfExists { get; set; }

        private static ConnectionSettings _instance = null;

        public static ConnectionSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Newtonsoft.Json.JsonConvert.DeserializeObject<ConnectionSettings>(System.IO.File.ReadAllText("connectionsettings.json"));

                return _instance;
            }
        }

    }
}
