using CY_ORM.SqlQuery;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;

namespace CY_ORM
{
    public static class QueryContext
    {
        internal enum DatabaseType
        {
            SqlServer,
            SqlServerCe,
            SqLite,
            MySql,
            Access,
            PostgreSql,
            Oracle,
        }

        #region 获取Query

        internal static InsertQuery<T> GetInsertQuery<T>(DbConnection conn, DbTransaction tran = null) where T : class
        {
            DatabaseType dbtype = GetDatabaseType(conn);

            switch (dbtype)
            {
                case DatabaseType.SqlServer:
                    return new SqlServerInsertQuery<T>(conn, tran);
                case DatabaseType.SqLite:
                    return new SqliteInsertQuery<T>(conn, tran);
            }
            return null;
        }

        internal static DeleteQuery<T> GetDeleteQuery<T>(DbConnection conn, DbTransaction tran = null) where T : class
        {
            DatabaseType dbtype = GetDatabaseType(conn);

            switch (dbtype)
            {
                case DatabaseType.SqlServer:
                    return new SqlServerDeleteQuery<T>(conn, tran);
                case DatabaseType.SqLite:
                    return new SqliteDeleteQuery<T>(conn, tran);
            }
            return null;
        }

        internal static UpdateQuery<T> GetUpdateQuery<T>(DbConnection conn, DbTransaction tran = null) where T : class
        {
            DatabaseType dbtype = GetDatabaseType(conn);
            switch (dbtype)
            {
                case DatabaseType.SqlServer:
                    return new SqlServerUpdateQuery<T>(conn, tran);
                case DatabaseType.SqLite:
                    return new SqliteUpdateQuery<T>(conn, tran);
            }
            return null;
        }

        internal static SelectQuery<T> GetSelectQuery<T>(DbConnection conn, DbTransaction tran = null) where T : class
        {
            DatabaseType dbtype = GetDatabaseType(conn);
            switch (dbtype)
            {
                case DatabaseType.SqlServer:
                    return new SqlServerSelectQuery<T>(conn, tran);
                case DatabaseType.SqLite:
                    return new SqliteSelectQuery<T>(conn, tran);
            }
            return null;
        }


        private readonly static ConcurrentDictionary<string, DatabaseType> DatabaseTypeDictionary = new ConcurrentDictionary<string, DatabaseType>();
        internal static DatabaseType GetDatabaseType(DbConnection conn)
        {
            DatabaseType res = DatabaseType.SqlServer;
            string dbname = conn.GetType().FullName;
            if (DatabaseTypeDictionary.ContainsKey(dbname))
            {
                res = DatabaseTypeDictionary[dbname];
            }
            else
            {
                if (dbname.IndexOf("SqlClient", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    res= DatabaseType.SqlServer;
                }
                else if (dbname.IndexOf(DatabaseType.MySql.ToString(), StringComparison.OrdinalIgnoreCase) != -1)
                {
                    res = DatabaseType.MySql;
                }
                else if (dbname.IndexOf(DatabaseType.SqLite.ToString(), StringComparison.OrdinalIgnoreCase) != -1)
                {
                    res = DatabaseType.SqLite;
                }
                else if (dbname.IndexOf(DatabaseType.SqlServerCe.ToString(), StringComparison.OrdinalIgnoreCase) != -1)
                {
                    res = DatabaseType.SqlServerCe;
                }
                else if (dbname.IndexOf("OleDb", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    res = DatabaseType.Access;
                }
                else if (dbname.IndexOf("Npgsql", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    res = DatabaseType.PostgreSql;
                }
                else if (dbname.IndexOf(DatabaseType.Oracle.ToString(), StringComparison.OrdinalIgnoreCase) != -1)
                {
                    res = DatabaseType.Oracle;
                }
                DatabaseTypeDictionary[dbname] = res;
            }
            return res;
        }

        #endregion

        #region Insert操作

        public static dynamic Insert<T>(this DbConnection conn, T obj, params Expression<Func<T, dynamic>>[] exprs) where T : class
        {
            return GetInsertQuery<T>(conn).Excute(obj, exprs);
        }

        public static dynamic Insert<T>(this DbTransaction tran, T obj, params Expression<Func<T, dynamic>>[] exprs) where T : class
        {
            return GetInsertQuery<T>(tran.Connection, tran).Excute(obj, exprs);
        }

        public static dynamic Insert<T>(this DbConnection conn, IEnumerable<T> objs) where T : class
        {
            return GetInsertQuery<T>(conn).ExcuteList(objs);
        }

        public static dynamic Insert<T>(this DbTransaction tran, IEnumerable<T> objs) where T : class
        {
            return GetInsertQuery<T>(tran.Connection, tran).ExcuteList(objs);
        }

        public static dynamic Insert<T>(this DbConnection conn, params Expression<Func<T, bool>>[] exprs) where T : class
        {
            return GetInsertQuery<T>(conn).Excute(exprs);
        }

        public static dynamic Insert<T>(this DbTransaction tran, params Expression<Func<T, bool>>[] exprs) where T : class
        {
            return GetInsertQuery<T>(tran.Connection, tran).Excute(exprs);
        }

        #endregion

        #region Delete操作

        public static dynamic Delete<T>(this DbConnection conn, Expression<Func<T, bool>> expr) where T : class
        {
            return GetDeleteQuery<T>(conn).Where(expr);
        }

        public static dynamic Delete<T>(this DbTransaction tran, Expression<Func<T, bool>> expr) where T : class
        {
            return GetDeleteQuery<T>(tran.Connection, tran).Where(expr);
        }

        public static dynamic Delete<T>(this DbConnection conn, T obj) where T : class
        {
            return GetDeleteQuery<T>(conn).Excute(obj);
        }

        public static dynamic Delete<T>(this DbTransaction tran, T obj) where T : class
        {
            return GetDeleteQuery<T>(tran.Connection, tran).Excute(obj);
        }

        public static dynamic Delete<T>(this DbConnection conn, IEnumerable<T> objs) where T : class
        {
            return GetDeleteQuery<T>(conn).ExcuteList(objs);
        }

        public static dynamic Delete<T>(this DbTransaction tran, IEnumerable<T> objs) where T : class
        {
            return GetDeleteQuery<T>(tran.Connection, tran).ExcuteList(objs);
        }

        #endregion

        #region Update操作

        public static dynamic Update<T>(this DbConnection conn, Expression<Func<T, bool>> wexpr, params Expression<Func<T, bool>>[] sexprs) where T : class
        {
            return GetUpdateQuery<T>(conn).Set(sexprs).Where(wexpr);
        }

        public static dynamic Update<T>(this DbTransaction tran, Expression<Func<T, bool>> wexpr, params Expression<Func<T, bool>>[] sexprs) where T : class
        {
            return GetUpdateQuery<T>(tran.Connection, tran).Set(sexprs).Where(wexpr);
        }

        public static dynamic Update<T>(this DbConnection conn, T obj, params Expression<Func<T, dynamic>>[] exprs) where T : class
        {
            return GetUpdateQuery<T>(conn).Excute(obj, exprs);
        }

        public static dynamic Update<T>(this DbTransaction tran, T obj, params Expression<Func<T, dynamic>>[] exprs) where T : class
        {
            return GetUpdateQuery<T>(tran.Connection, tran).Excute(obj, exprs);
        }
        public static dynamic Update<T>(this DbConnection conn, IEnumerable<T> objs, params Expression<Func<T, dynamic>>[] exprs) where T : class
        {
            return GetUpdateQuery<T>(conn).ExcuteList(objs, exprs);
        }

        public static dynamic Update<T>(this DbTransaction tran, IEnumerable<T> objs, params Expression<Func<T, dynamic>>[] exprs) where T : class
        {
            return GetUpdateQuery<T>(tran.Connection, tran).ExcuteList(objs, exprs);
        }

        #endregion

        #region Select操作

        public static SelectQuery<T> Select<T>(this DbConnection conn, params Expression<Func<T, dynamic>>[] exprs) where T : class
        {
            return GetSelectQuery<T>(conn).Select(exprs);
        }

        public static SelectQuery<T> Select<T>(this DbTransaction tran, params Expression<Func<T, dynamic>>[] exprs) where T : class
        {
            return GetSelectQuery<T>(tran.Connection, tran).Select(exprs);
        }

        #endregion
    }
}

