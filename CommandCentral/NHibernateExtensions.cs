using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using System.Linq.Expressions;
using FluentNHibernate.Utils;

namespace CommandCentral
{
    public static class NHibernateExtensions
    {

        public static PropertyPart For<T>(Expression<Func<T, object>> memberExpression)
        {
            return new PropertyPart(memberExpression.ToMember(), typeof(T));
        }



    }
}
