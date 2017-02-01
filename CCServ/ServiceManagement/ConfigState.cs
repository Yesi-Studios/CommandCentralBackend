using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.CustomTypes;
using FluentNHibernate.Mapping;
using AtwoodUtils;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CCServ.ServiceManagement
{
    /// <summary>
    /// Stores a config state and maps a config state to the database.
    /// </summary>
    public class ConfigState
    {
        #region Properties

        /// <summary>
        /// The address at which the DOD's SMTP server can be found.  This is the address through which we should send emails.
        /// </summary>
        public string DODSMTPAddress { get; set; }

        /// <summary>
        /// The email address of the developer distro from which all emails should be sent and CCed.
        /// </summary>
        public string DeveloperDistroAddress { get; set; }

        /// <summary>
        /// The display name for the developer distro.
        /// </summary>
        public string DeveloperDistroDisplayName { get; set; }

        /// <summary>
        /// The personal email addresses of the developers, used for error handling and such.
        /// </summary>
        public List<string> DeveloperPersonalAddresses { get; set; }

        /// <summary>
        /// The email host of a DOD email address.
        /// </summary>
        public string DODEmailHost { get; set; }

        /// <summary>
        /// The time at which a muster should rollover.
        /// </summary>
        public Time MusterRolloverTime { get; set; }

        /// <summary>
        /// The time by which the muster is expected to have been completed.
        /// </summary>
        public Time MusterDueTime { get; set; }

        /// <summary>
        /// Indicates if the muster is in a finalized state.
        /// </summary>
        public bool IsMusterFinalized { get; set; }

        /// <summary>
        /// The max age of a profile lock.
        /// </summary>
        public TimeSpan ProfileLockMaxAge { get; set; }

        /// <summary>
        /// The current version of the application.
        /// </summary>
        public string Version { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns a default config object.
        /// </summary>
        /// <returns></returns>
        public static ConfigState GetDefault()
        {
            return new ConfigState
            {
                DeveloperPersonalAddresses = new List<string>
                {
                    "sundevilgoalie13@gmail.com",
                    "anguslmm@gmail.com",
                    "brandy.stiverson@gmail.com"
                },
                DeveloperDistroAddress = "usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil",
                DeveloperDistroDisplayName = "Command Central Communications",
                DODEmailHost = "mail.mil",
                DODSMTPAddress = "smtp.gordon.army.mil",
                IsMusterFinalized = false,
                MusterDueTime = new Time(13, 30, 00),
                MusterRolloverTime = new Time(20, 0, 0),
                ProfileLockMaxAge = TimeSpan.FromMinutes(20),
                Version = "1.0.0"
            };
        }

        #endregion

        #region Startup Method

        /// <summary>
        /// Loads the config from the config file.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 100)]
        private static void LoadConfigState(CLI.Options.LaunchOptions launchOptions)
        {
            //Alright, we're going to try to load the config.  If anything fails, or happens along the way, we'll replace the config file with a default one.
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.txt");

            if (!File.Exists(path))
            {
                var defaultConfig = ConfigState.GetDefault();

                File.WriteAllText(path, JsonConvert.SerializeObject(defaultConfig,
                    new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } },
                        ContractResolver = new AtwoodUtils.SerializationSettings.NHibernateContractResolver(),
                        Formatting = Formatting.Indented,
                        DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    }));

                ServiceManagement.ServiceManager.CurrentConfigState = defaultConfig;

                "The config file didn't exist!  A new one was created at '{0}'!".FormatS(path).WriteLine();
            }
            else
            {
                //Ok so the file exists, let's get its text.
                var rawText = File.ReadAllText(path);

                //Now let's try to turn it into a config state object. 
                try
                {
                    var configState = JsonConvert.DeserializeObject<ConfigState>(rawText);

                    if (configState == null)
                    {
                        //Something went wrong, let's redo the config file.
                        var defaultConfig = ConfigState.GetDefault();

                        File.WriteAllText(path, JsonConvert.SerializeObject(defaultConfig,
                            new JsonSerializerSettings
                            {
                                Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } },
                                ContractResolver = new AtwoodUtils.SerializationSettings.NHibernateContractResolver(),
                                Formatting = Formatting.Indented,
                                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
                                DateTimeZoneHandling = DateTimeZoneHandling.Utc
                            }));

                        ServiceManagement.ServiceManager.CurrentConfigState = defaultConfig;

                        "Something was wrong with the config file.  It was overwritten with the default config.".WriteLine();
                    }
                    else
                    {
                        ServiceManagement.ServiceManager.CurrentConfigState = configState;

                        "Config file was loaded!.".WriteLine();
                    }
                }
                catch
                {
                    //Something went wrong, let's redo the config file.
                    var defaultConfig = ConfigState.GetDefault();

                    File.WriteAllText(path, JsonConvert.SerializeObject(defaultConfig,
                        new JsonSerializerSettings
                        {
                            Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } },
                            ContractResolver = new AtwoodUtils.SerializationSettings.NHibernateContractResolver(),
                            Formatting = Formatting.Indented,
                            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc
                        }));

                    ServiceManagement.ServiceManager.CurrentConfigState = defaultConfig;

                    "Something was wrong with the config file.  It was overwritten with the default config.".WriteLine();
                }
            }
        }

        #endregion

    }
}
