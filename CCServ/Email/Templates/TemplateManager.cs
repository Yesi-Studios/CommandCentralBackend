using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Logging;

namespace CCServ.Email.Templates
{
    /// <summary>
    /// Contains all the email templates and their loader.
    /// </summary>
    public static class TemplateManager
    {
        /// <summary>
        /// The list of all email templates.  The key is the name of the template.  i.e. template.html 
        /// </summary>
        public static ConcurrentDictionary<string, string> AllTemplates = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Loads all email templates into memory for faster retrieval.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 9)]
        private static void LoadEmailTemplates(CLI.Options.LaunchOptions options)
        {
            //First up, go and get all email templates in this same namespace.
            var names = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().Where(x => x.Contains(typeof(TemplateManager).Namespace) && x.Contains("html"));

            foreach (var name in names)
            {
                using (var stream = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream(name))
                {
                    if (stream == null)
                    {
                        Log.Warning("The email template, '{0}', was not loaded successfully.".FormatS(name));
                    }
                    else
                    {
                        using (var reader = new System.IO.StreamReader(stream))
                        {
                            var elements = name.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            var fileName = String.Format("{0}.{1}", elements[elements.Count - 2], elements[elements.Count - 1]);
                            AllTemplates.AddOrUpdate(fileName, reader.ReadToEnd(), (key, value) =>
                            {
                                Log.Warning("The resource, {0}, was loaded twice.  The most recent version was kept.".FormatS(fileName));
                                return value;
                            });
                        }
                    }
                }
            }
        }
    }
}
