using CY_ORM.Mapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace CY_ORM.SqlQuery
{
    public abstract class Query<T> where T : class
    {
        public Query(DbConnection conn, DbTransaction tran = null)
        {
            Connection = conn;
            Transaction = tran;
            Mapper = MapperHelper.Get<T>();
        }

        internal DbConnection Connection { get; set; }
        internal DbTransaction Transaction { get; set; }
        internal ClassMapper Mapper { get; set; }
        internal bool IsDebug { get; set; }
        public string DebugSql { get; set; }

        internal abstract string GetSql();

        protected string EmptyWhere
        {
            get { return "1=1"; }
        }
    }

    public abstract class InsertQuery<T> : Query<T> where T : class
    {
        protected InsertQuery(DbConnection conn, DbTransaction tran = null)
            : base(conn, tran)
        {
            InsertColumnStr = new StringBuilder();
            InsertValueStr = new StringBuilder();
            DapperParameters = new DynamicParameters();
        }

        internal StringBuilder InsertColumnStr { get; set; }
        internal StringBuilder InsertValueStr { get; set; }
        internal DynamicParameters DapperParameters { get; set; }

        internal abstract dynamic Excute(T obj, params Expression<Func<T, dynamic>>[] exprs);
        internal abstract dynamic ExcuteList(IEnumerable<T> p_Objs);
        internal abstract dynamic Excute(params Expression<Func<T, bool>>[] exprs);

        protected abstract string GetIdentitySql();
    }

    public abstract class DeleteQuery<T> : Query<T> where T : class
    {
        protected DeleteQuery(DbConnection conn, DbTransaction tran = null)
            : base(conn, tran)
        {
            WhereStr = new StringBuilder(EmptyWhere);
            DapperParameters = new DynamicParameters();
        }

        internal StringBuilder WhereStr { get; set; }
        internal DynamicParameters DapperParameters { get; set; }

        internal abstract dynamic Excute(T obj);
        internal abstract dynamic ExcuteList(IEnumerable<T> obj);
        internal abstract dynamic Where(Expression<Func<T, bool>> expr);
    }

    public abstract class UpdateQuery<T> : Query<T> where T : class
    {
        protected UpdateQuery(DbConnection conn, DbTransaction tran = null)
            : base(conn, tran)
        {
            SetStr = new StringBuilder();
            WhereStr = new StringBuilder(EmptyWhere);
            DapperParameters = new DynamicParameters();
        }

        internal StringBuilder SetStr { get; set; }
        internal StringBuilder WhereStr { get; set; }
        internal DynamicParameters DapperParameters { get; set; }

        internal abstract UpdateQuery<T> Set(params Expression<Func<T, bool>>[] exprs);
        internal abstract dynamic Where(Expression<Func<T, bool>> expr);

        internal abstract dynamic Excute(T obj, params Expression<Func<T, dynamic>>[] exprs);
        internal abstract dynamic ExcuteList(IEnumerable<T> obj, params Expression<Func<T, dynamic>>[] exprs);
    }

    public abstract class SelectQuery<T> : Query<T> where T : class
    {
        protected SelectQuery(DbConnection conn, DbTransaction tran = null)
            : base(conn, tran)
        {

            SelectStr = new StringBuilder();
            OrderByStr = new StringBuilder();
            GroupByStr = new StringBuilder();
            WhereStr = new StringBuilder(EmptyWhere);
            DapperParameters = new DynamicParameters();
            PageIndex = 0;
            PageSize = 0;
        }

        internal StringBuilder SelectStr { get; set; }
        internal StringBuilder OrderByStr { get; set; }
        internal StringBuilder GroupByStr { get; set; }
        internal StringBuilder WhereStr { get; set; }
        internal DynamicParameters DapperParameters { get; set; }

        internal long PageIndex { get; set; }
        internal long PageSize { get; set; }

        public SelectQuery<T> Debug()
        {
            IsDebug = true;
            return this;
        }

        internal abstract SelectQuery<T> Select(params Expression<Func<T, dynamic>>[] exprs);
        public abstract SelectQuery<T> Where(Expression<Func<T, bool>> expr = null);
        public abstract SelectQuery<T> OrderBy(params Expression<Func<T, OrderType>>[] exprs);
        public abstract SelectQuery<T> GroupBy(params Expression<Func<T, dynamic>>[] exprs);

        #region 返回结果方法

        public abstract List<T> ToList(long top = -1);
        public abstract T ToSingle();
        public abstract PageList<T> ToPageList(long pageindex, int pagesize);
        public abstract int ToCount();
        public abstract long ToLongCount();

        #endregion

        internal abstract string GetSelect();
        internal abstract string GetJoin();
        internal abstract string GetOrderBy();
        internal abstract string GetGroupBy();
        internal abstract string GetCount();
    }
}