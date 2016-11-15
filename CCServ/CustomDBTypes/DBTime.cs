using AtwoodUtils;
using CustomDBTypes;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.CustomDBTypes
{
    /// <summary>
    /// Represents a Time with no date because, for whatever reason, the .NET framework doesn't have that.
    /// </summary>
    public class DBTime : IUserType
    {

        public object Assemble(object cached, object owner)
        {
            return DeepCopy(cached);
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Disassemble(object value)
        {
            return DeepCopy(value);
        }

        public bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }
            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public bool IsMutable
        {
            get { return true; }
        }

        public object NullSafeGet(System.Data.IDataReader rs, string[] names, object owner)
        {
            var valueToGet = NHibernateUtil.String.NullSafeGet(rs, names[0]) as string;
            Time returnValue;
            Time.TryParse(valueToGet, out returnValue);
            return returnValue;
        }

        public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index)
        {
            object valueToSet = ((Time)value).ToString();
            NHibernateUtil.String.NullSafeSet(cmd, valueToSet, index);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(Time); }
        }

        public NHibernate.SqlTypes.SqlType[] SqlTypes
        {
            get { return new[] { new SqlType(DbType.String) }; }
        }
    }
}
