using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.PreDefs
{
    public class PreDefOf<T> : IPreDef where T : class
    {

        public new string TypeFullName
        {
            get
            {
                return typeof(T).FullName;
            }
        }

        public List<T> Definitions { get; set; }

        public static PreDefOf<T> Get()
        {
            return (PreDefOf<T>)PreDefUtility.Predefs.FirstOrDefault(x => x.TypeFullName == typeof(T).FullName);
        }

    }
}
