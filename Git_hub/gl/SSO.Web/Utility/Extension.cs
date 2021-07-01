using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace SSO.Web.Utility
{
    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return System.Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

    public static class Extension
    {
        public static IQueryable<T> Where<T>
            (this IQueryable<T> source, Expression<Func<T, bool>> predicate, bool flag)
        {
            if (flag)
                return source.Where(predicate);
            return source;
        }

        public static IEnumerable<T> Where<T>
            (this IEnumerable<T> source, Func<T, bool> predicate, bool flag)
        {
            if (flag)
                return source.Where(predicate);
            return source;
        }
    }
}