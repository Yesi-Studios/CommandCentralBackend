using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.EmailInterface
{
    internal static class TemplateHelper
    {
        private static ConcurrentDictionary<string, string> _compiledTemplates = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Renders the given template.
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="model"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string RenderTemplate(string resourcePath, object model, Assembly assembly)
        {

            if (!_compiledTemplates.TryGetValue(resourcePath, out string template))
            {
                using (var stream = assembly.GetManifestResourceStream(resourcePath))
                using (var reader = new StreamReader(stream))
                {
                    var newTemplate = reader.ReadToEnd();

                    _compiledTemplates.TryAdd(resourcePath, newTemplate);
                    template = newTemplate;
                }
            }

            if (template == null)
                throw new ArgumentNullException("template");

            if (model == null)
                throw new ArgumentNullException("model");

            return RazorEngine.Razor.Parse(template, model);
        }
    }
}
