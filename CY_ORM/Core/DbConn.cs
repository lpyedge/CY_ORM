using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

namespace CY_ORM
{
    public static class DbConn
    {
        //大量读取,少量修改和删除 使用SortedList 其他情况下使用Dictionary
        private static readonly SortedList<string, Func<DbConnection>> Funcdic = new SortedList<string, Func<DbConnection>>();
        private static readonly SortedList<string, string> Connstrdic = new SortedList<string, string>();

        private static Func<DbConnection> func;
        private static string connstr;

        public static void Set<T>(string p_connstr, string p_name = null)
        {
            if (p_name == null)
            {
                func = CreateFunc(typeof(T));
                connstr = p_connstr;
            }
            else
            {
                Funcdic[p_name] = CreateFunc(typeof(T));
                Connstrdic[p_name] = p_connstr;
            }
        }

        public static DbConnection Get(string p_name)
        {
            if (Funcdic.ContainsKey(p_name) && Connstrdic.ContainsKey(p_name))
            {
                var conn = Funcdic[p_name]();
                conn.ConnectionString = Connstrdic[p_name];
                conn.Open();
                return conn;
            }
            return null;
        }
        public static DbConnection Instance
        {
            get
            {
                if (func != null && connstr != null)
                {
                    var conn = func();
                    conn.ConnectionString = connstr;
                    conn.Open();
                    return conn;
                }
                return null;

            }
        }

        private static Func<DbConnection> CreateFunc(Type p_type, params string[] args)
        {
            NewExpression newExpression = Expression.New(p_type);
            return Expression.Lambda<Func<DbConnection>>(newExpression, null).Compile();
        }
    }
}
