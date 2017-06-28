using AtwoodUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.PreDefs
{
    public static class PreDefUtility
    {

        public static ConcurrentBag<IPreDef> Predefs = new ConcurrentBag<IPreDef>();

        static PreDefUtility()
        {
            var test = Assembly.GetExecutingAssembly().GetName().Name;

            var preDefNames = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(x => x.StartsWith("{0}.PreDefs".With(Assembly.GetExecutingAssembly().GetName().Name)) && x.EndsWith(".cc"));

            foreach (var resourceName in preDefNames)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();

                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);

                        var fullName = jObject.Value<string>(nameof(IPreDef.TypeFullName));

                        var type = Assembly.GetExecutingAssembly().GetType(fullName, true);

                        Predefs.Add((IPreDef)jObject.ToObject(typeof(PreDefOf<>).MakeGenericType(type)));

                        Logging.Log.Info("Loaded PreDef for type {0}".With(fullName));
                    }
                }
            }
        }

        public static void PersistPreDef<T>() where T : class
        {

            var predef = (PreDefOf<T>)Predefs.FirstOrDefault(x => x.TypeFullName == typeof(T).FullName) ??
                throw new Exception("{0} does not exist.".With(typeof(T).FullName));

            PersistPreDef(predef);
        }

        public static void PersistPreDef<T>(PreDefOf<T> preDef) where T : class
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    foreach (var item in preDef.Definitions)
                    {
                        session.Save(item);
                    }

                    transaction.Commit();
                }

                Logging.Log.Info("Persisted {0} defs for PreDef {1}.".With(preDef.Definitions.Count, preDef.TypeFullName));
            }
        }
    }
}
