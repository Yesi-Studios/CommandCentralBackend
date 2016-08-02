using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using AtwoodUtils;

namespace CommandCentral.Authorization.Groups
{
    public static class PropertySelector
    {
        public static List<MemberInfo> Properties<T, PropertyT>(params Expression<Func<T, PropertyT>>[] expressions)
        {
            return expressions.Select(x => x.GetProperty()).ToList();
        }
    }
}
