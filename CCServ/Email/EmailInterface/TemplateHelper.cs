using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.EmailInterface
{
    internal static class TemplateHelper
    {
        private static Dictionary<string, string> _compiledTemplates = new Dictionary<string, string>();

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

                    _compiledTemplates.Add(resourcePath, newTemplate);
                    template = newTemplate;
                }
            }

            return RazorEngine.Razor.Parse(template, model);
        }


    }
}
