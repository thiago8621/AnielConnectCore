using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Data;
using System.Data.Common;

namespace Sinapse.DAO.AspNetCore.map
{
    public class BooleanMapType : IUserType
    {
        public SqlType[] SqlTypes
        {
            get { return new[] { NHibernateUtil.String.SqlType }; }
        }

        public Type ReturnedType
        {
            get { return typeof(bool); }
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public object Assemble(object cached, object owner) => cached;
        public object DeepCopy(object value) => value;
        public object Disassemble(object value) => value;
        public int GetHashCode(object x) => x == null ? typeof(bool).GetHashCode() + 473 : x.GetHashCode();
        public object Replace(object original, object target, object owner) => original;

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var obj = NHibernateUtil.String.NullSafeGet(rs, names[0], session);
            if (obj == null) return null;
            var yesNo = (string)obj;
            if (yesNo != "S" && yesNo != "N") return false;
            return yesNo == "S";
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if (value == null) ((IDataParameter)cmd.Parameters[index]).Value = DBNull.Value;
            else
            {
                var yes = (bool)value;
                ((IDataParameter)cmd.Parameters[index]).Value = yes ? "S" : "N";
            }
        }
    }
}