using CY_ORM.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CY_ORM.SqlQuery
{
    public class SqlServerInsertQuery<T> : InsertQuery<T> where T : class
    {
        public SqlServerInsertQuery(DbConnection conn, DbTransaction tran = null)
            : base(conn, tran)
        {

        }


        internal override string GetSql()
        {
            return string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", Mapper.TableName, InsertColumnStr.ToString(),
                InsertValueStr.ToString());
        }

        internal override dynamic Excute(T obj, params Expression<Func<T, dynamic>>[] exprs)
        {
            if (exprs.Length > 0)
            {
                var p = new ParserArgs { Mapper = Mapper };
                foreach (var expr in exprs)
                {
                    if (expr.Body is MemberExpression | (expr.Body is UnaryExpression && ((UnaryExpression)expr.Body).Operand is MemberExpression))
                    {
                        LambdaParser.InsertColumn(expr.Body, p);
                        InsertValueStr.Append(string.Format("@i{0},", DapperParameters.ParameterNames.Count()));
                        DapperParameters.Add(string.Format("i{0}", DapperParameters.ParameterNames.Count()),
                            Reflection.Property.GetValue(obj, LambdaParser.MemberName(expr)));
                    }
                    else
                    {
                        throw new Exception("非法的INSERT内容!");
                    }
                }
                InsertColumnStr = p.Builder.Remove(p.Builder.Length - 1, 1);
                InsertValueStr = InsertValueStr.Remove(InsertValueStr.Length - 1, 1);
            }
            else
            {
                foreach (var propertyMap in Mapper.InsertProperties.Values)
                {
                    InsertColumnStr.Append(propertyMap.Select);
                    InsertColumnStr.Append(",");
                    InsertValueStr.Append(string.Format("@i{0},", DapperParameters.ParameterNames.Count()));
                    DapperParameters.Add(string.Format("i{0}", DapperParameters.ParameterNames.Count()),
                        Reflection.Property.GetValue(obj, propertyMap.Name));
                }
                InsertColumnStr = InsertColumnStr.Remove(InsertColumnStr.Length - 1, 1);
                InsertValueStr = InsertValueStr.Remove(InsertValueStr.Length - 1, 1);
            }
            if (Connection.Execute(GetSql(), DapperParameters, Transaction) > 0)
            {
                var id = (IDictionary<string, object>)Connection.Query<CY_ORM.SqlMapper.DapperRow>(GetIdentitySql(), null, Transaction).FirstOrDefault();
                return (id != null && id.Count > 0) ? id.Values.FirstOrDefault() : null;
            }
            return null;
        }

        internal override dynamic Excute(params Expression<Func<T, bool>>[] exprs)
        {
            if (exprs.Length > 0)
            {
                var p1 = new ParserArgs();
                var p2 = new ParserArgs();
                p1.Mapper = Mapper;
                p2.Mapper = Mapper;
                foreach (var expr in exprs)
                {
                    LambdaParser.InsertColumn(expr.Body, p1);
                    LambdaParser.InsertValue(expr.Body, p2);
                }
                InsertColumnStr = p1.Builder.Remove(p1.Builder.Length - 1, 1);
                InsertValueStr = p2.Builder.Remove(p2.Builder.Length - 1, 1);
                foreach (var item in p2.Parameters)
                {
                    DapperParameters.Add(item.Key, item.Value);
                }

                if (Connection.Execute(GetSql(), DapperParameters, Transaction) > 0)
                {
                    var id = (IDictionary<string, object>)Connection.Query<CY_ORM.SqlMapper.DapperRow>(GetIdentitySql(), null, Transaction).FirstOrDefault();
                    return (id != null && id.Count > 0) ? id.Values.FirstOrDefault() : null;
                }
            }
            return null;
        }

        protected override string GetIdentitySql()
        {
            return string.Format("SELECT MAX({0}) AS [Id] FROM [{1}]", Mapper.KeyProperties.Values.First().Select, Mapper.TableName);
        }

        private DataTable Serialize(IEnumerable<T> list)
        {
            // 定义集合
            var res = new DataTable(Mapper.TableName);

            foreach (var pi in Mapper.InsertProperties.Values)
            {
                // 检查DataTable是否包含此列
                if (!res.Columns.Contains(pi.ColumnName))
                {
                    res.Columns.Add(pi.ColumnName, pi.PropertyInfo.PropertyType);
                }
            }
            if (list == null) return res;

            foreach (var item in list)
            {
                var dr = res.NewRow();
                foreach (var pi in Mapper.InsertProperties.Values)
                {
                    dr[pi.ColumnName] = Reflection.Property.GetValue(item, pi.ColumnName);
                }
                res.Rows.Add(dr);
            }

            return res;
        }

        internal override dynamic ExcuteList(IEnumerable<T> objs)
        {
            var pList = objs as T[] ?? objs.ToArray();
            if (objs != null && pList.Length > 0)
            {
                int rowcount = pList.Length;
                //if (Transaction == null && rowcount > 200)
                //{
                //    //数据批量导入sqlserver,创建实例    SqlBulkCopyOptions.UseInternalTransaction采用事务  复制失败自动回滚
                //    using (var sqlbulk = new SqlBulkCopy((SqlConnection)Connection, SqlBulkCopyOptions.UseInternalTransaction, (SqlTransaction)Transaction))
                //    {
                //        sqlbulk.NotifyAfter = rowcount;
                //        //目标数据库表名
                //        sqlbulk.DestinationTableName = "[" + Mapper.TableName + "]";

                //        //数据集字段索引与数据库字段索引映射
                //        int i = 0;
                //        foreach (var pi in Mapper.InsertProperties.Values)
                //        {
                //            sqlbulk.ColumnMappings.Add(i, pi.ColumnName);
                //            i++;
                //        }

                //        //导入
                //        sqlbulk.WriteToServer(Serialize(pList));
                //    }

                //    var id = (IDictionary<string, object>)Connection.Query<CY_ORM.SqlMapper.DapperRow>(GetIdentitySql(), null, Transaction).FirstOrDefault();
                //    return (id != null && id.Count > 0) ? id.Values.FirstOrDefault() : null;
                //}
                //else
                //{
                    int index = 0;
                    foreach (var propertyMap in Mapper.InsertProperties.Values)
                    {
                        InsertColumnStr.Append(propertyMap.Select);
                        InsertColumnStr.Append(",");
                        InsertValueStr.Append(string.Format("@i{0},", index));
                        index++;
                    }
                    InsertColumnStr = InsertColumnStr.Remove(InsertColumnStr.Length - 1, 1);
                    InsertValueStr = InsertValueStr.Remove(InsertValueStr.Length - 1, 1);

                    foreach (var obj in pList)
                    {
                        var parameters = new DynamicParameters();
                        foreach (var propertyMap in Mapper.InsertProperties.Values)
                        {
                            parameters.Add(string.Format("i{0}", parameters.ParameterNames.Count()),
                                Reflection.Property.GetValue(obj, propertyMap.Name));
                        }
                        Connection.Execute(GetSql(), parameters, Transaction);
                    }

                    var id = (IDictionary<string, object>)Connection.Query<CY_ORM.SqlMapper.DapperRow>(GetIdentitySql(), null, Transaction).FirstOrDefault();
                    return (id != null && id.Count > 0) ? id.Values.FirstOrDefault() : null;
                //}

            }
            return null;
        }
    }

    public class SqlServerDeleteQuery<T> : DeleteQuery<T> where T : class
    {
        public SqlServerDeleteQuery(DbConnection conn, DbTransaction tran = null)
            : base(conn, tran)
        {

        }

        internal override string GetSql()
        {
            return string.Format("DELETE FROM [{0}] WHERE {1}", Mapper.TableName,
                WhereStr.ToString());
        }

        internal override dynamic Excute(T obj)
        {
            if (obj != null)
            {
                WhereStr = new StringBuilder();
                foreach (var propertyMap in Mapper.KeyProperties.Values)
                {
                    WhereStr.Append(propertyMap.Where);
                    WhereStr.Append('=');
                    WhereStr.Append(string.Format("@w{0} AND ", DapperParameters.ParameterNames.Count()));
                    DapperParameters.Add(string.Format("w{0}", DapperParameters.ParameterNames.Count()),
                        Reflection.Property.GetValue(obj, propertyMap.Name));
                }
                WhereStr = WhereStr.Remove(WhereStr.Length - 5, 5);
                return Connection.Execute(GetSql(), DapperParameters, Transaction);
            }
            return 0;
        }

        internal override dynamic ExcuteList(IEnumerable<T> objs)
        {
            var pList = objs as T[] ?? objs.ToArray();
            if (objs != null && pList.Length > 0)
            {
                int index = 0;
                WhereStr = new StringBuilder();
                foreach (var propertyMap in Mapper.KeyProperties.Values)
                {
                    WhereStr.Append(propertyMap.Where);
                    WhereStr.Append('=');
                    WhereStr.Append(string.Format("@w{0} AND ", index));
                    index++;
                }
                WhereStr = WhereStr.Remove(WhereStr.Length - 5, 5);

                foreach (var obj in pList)
                {
                    var parameters = new DynamicParameters();
                    foreach (var propertyMap in Mapper.KeyProperties.Values)
                    {
                        parameters.Add(string.Format("w{0}", parameters.ParameterNames.Count()),
                            Reflection.Property.GetValue(obj, propertyMap.Name));
                    }
                    Connection.Execute(GetSql(), parameters, Transaction);
                }
                return pList.Length;
            }
            return 0;
        }

        internal override dynamic Where(Expression<Func<T, bool>> expr)
        {
            var p = new ParserArgs { Mapper = Mapper };
            LambdaParser.Where(expr.Body, p);
            WhereStr = p.Builder;
            foreach (var item in p.Parameters)
            {
                DapperParameters.Add(item.Key, item.Value);
            }
            return Connection.Execute(GetSql(), DapperParameters, Transaction);
        }
    }

    public class SqlServerUpdateQuery<T> : UpdateQuery<T> where T : class
    {
        public SqlServerUpdateQuery(DbConnection conn, DbTransaction tran = null)
            : base(conn, tran)
        {

        }

        internal override string GetSql()
        {
            return string.Format("UPDATE [{0}] SET {1} WHERE {2}", Mapper.TableName, SetStr.ToString(),
                WhereStr.ToString());
        }

        internal override UpdateQuery<T> Set(params Expression<Func<T, bool>>[] exprs)
        {
            if (exprs != null && exprs.Length > 0)
            {
                var p = new ParserArgs { Mapper = Mapper };
                foreach (var expr in exprs)
                {
                    LambdaParser.UpdateSet(expr.Body, p);
                }
                SetStr = p.Builder.Remove(p.Builder.Length - 1, 1);
                foreach (var item in p.Parameters)
                {
                    DapperParameters.Add(item.Key, item.Value);
                }
                return this;
            }
            else
            {
                throw new Exception("调用Set方法必须设置修改内容！");
            }
        }

        internal override dynamic Where(Expression<Func<T, bool>> expr)
        {
            if (expr != null)
            {
                var p = new ParserArgs { Mapper = Mapper };
                LambdaParser.Where(expr.Body, p);
                WhereStr = p.Builder;
                foreach (var item in p.Parameters)
                {
                    DapperParameters.Add(item.Key, item.Value);
                }
                return Connection.Execute(GetSql(), DapperParameters, Transaction);
            }
            else
            {
                return Connection.Execute(GetSql(), DapperParameters, Transaction);
            }
        }

        internal override dynamic Excute(T obj, params Expression<Func<T, dynamic>>[] exprs)
        {
            if (exprs.Length > 0)
            {
                var p = new ParserArgs { Mapper = Mapper };
                foreach (var expr in exprs)
                {
                    if (expr.Body is MemberExpression | (expr.Body is UnaryExpression && ((UnaryExpression)expr.Body).Operand is MemberExpression))
                    {
                        LambdaParser.UpdateSet(expr.Body, p);
                        p.Builder.Append(string.Format("=@u{0},", DapperParameters.ParameterNames.Count()));
                        var name = LambdaParser.MemberName(expr);
                        DapperParameters.Add(string.Format("u{0}", DapperParameters.ParameterNames.Count()),
                            Reflection.Property.GetValue(obj, name));
                    }
                    else
                    {
                        throw new Exception("非法的SET内容!");
                    }
                }
                SetStr = p.Builder.Remove(p.Builder.Length - 1, 1);
            }
            else
            {
                foreach (var propertyMap in Mapper.UpdateProperties.Values)
                {
                    SetStr.Append(propertyMap.Where);//set
                    SetStr.Append('=');
                    SetStr.Append(string.Format("@u{0},", DapperParameters.ParameterNames.Count()));
                    DapperParameters.Add(string.Format("u{0}", DapperParameters.ParameterNames.Count()),
                        Reflection.Property.GetValue(obj, propertyMap.Name));
                }
                SetStr = SetStr.Remove(SetStr.Length - 1, 1);
            }
            WhereStr = new StringBuilder();
            foreach (var propertyMap in Mapper.KeyProperties.Values)
            {
                WhereStr.Append(propertyMap.Where);
                WhereStr.Append('=');
                WhereStr.Append(string.Format("@w{0} AND ", DapperParameters.ParameterNames.Count()));
                DapperParameters.Add(string.Format("w{0}", DapperParameters.ParameterNames.Count()),
                    Reflection.Property.GetValue(obj, propertyMap.Name));
            }
            WhereStr = WhereStr.Remove(WhereStr.Length - 5, 5);
            return Connection.Execute(GetSql(), DapperParameters, Transaction);
        }

        internal override dynamic ExcuteList(IEnumerable<T> objs, params Expression<Func<T, dynamic>>[] exprs)
        {
            var pList = objs as T[] ?? objs.ToArray();
            if (objs != null && pList.Length > 0)
            {
                int index = 0;
                if (exprs.Length > 0)
                {
                    var p = new ParserArgs { Mapper = Mapper };
                    foreach (var expr in exprs)
                    {
                        if (expr.Body is MemberExpression | (expr.Body is UnaryExpression && ((UnaryExpression)expr.Body).Operand is MemberExpression))
                        {
                            LambdaParser.UpdateSet(expr.Body, p);
                            p.Builder.Append(string.Format("=@u{0},", index));
                            index++;
                        }
                        else
                        {
                            throw new Exception("非法的SET内容!");
                        }
                    }
                    SetStr = p.Builder.Remove(p.Builder.Length - 1, 1);
                }
                else
                {
                    foreach (var propertyMap in Mapper.UpdateProperties.Values)
                    {
                        SetStr.Append(propertyMap.Where);//set
                        SetStr.Append('=');
                        SetStr.Append(string.Format("@u{0},", index));
                        index++;
                    }
                    SetStr = SetStr.Remove(SetStr.Length - 1, 1);
                }
                WhereStr = new StringBuilder();
                foreach (var propertyMap in Mapper.KeyProperties.Values)
                {
                    WhereStr.Append(propertyMap.Where);
                    WhereStr.Append('=');
                    WhereStr.Append(string.Format("@w{0} AND ", index));
                    index++;
                }
                WhereStr = WhereStr.Remove(WhereStr.Length - 5, 5);

                foreach (var obj in pList)
                {
                    var parameters = new DynamicParameters();
                    if (exprs.Length > 0)
                    {
                        foreach (var expr in exprs)
                        {
                            if (expr.Body is MemberExpression | (expr.Body is UnaryExpression && ((UnaryExpression)expr.Body).Operand is MemberExpression))
                            {
                                var name = LambdaParser.MemberName(expr);
                                parameters.Add(string.Format("u{0}", parameters.ParameterNames.Count()),
                                    Reflection.Property.GetValue(obj, name));
                            }
                            else
                            {
                                throw new Exception("非法的SET内容!");
                            }
                        }
                    }
                    else
                    {
                        foreach (var propertyMap in Mapper.UpdateProperties.Values)
                        {
                            parameters.Add(string.Format("u{0}", parameters.ParameterNames.Count()),
                                Reflection.Property.GetValue(obj, propertyMap.Name));
                        }
                    }

                    foreach (var propertyMap in Mapper.KeyProperties.Values)
                    {
                        parameters.Add(string.Format("w{0}", parameters.ParameterNames.Count()),
                            Reflection.Property.GetValue(obj, propertyMap.Name));
                    }
                    Connection.Execute(GetSql(), parameters, Transaction);
                }
                return pList.Length;
            }
            return 0;
        }
    }

    public class SqlServerSelectQuery<T> : SelectQuery<T> where T : class
    {
        public SqlServerSelectQuery(DbConnection conn, DbTransaction tran = null)
            : base(conn, tran)
        {
        }



        internal override string GetSql()
        {
            if (PageIndex > 0 && PageSize > 0)
            {
                if (PageIndex == 1)
                {
                    return string.Format("SELECT {1} FROM [{0}] {3} WHERE {2} {4} {5}", Mapper.TableName, GetSelect(), WhereStr.ToString(), GetJoin(), GetGroupBy(), GetOrderBy());
                }
                else
                {
                    return string.Format("SELECT {1} FROM [{0}] {3} WHERE {2} {4} {5}", Mapper.TableName, GetSelect(), WhereStr.ToString(), GetJoin(), GetGroupBy(), "");
                }
            }
            else
            {
                return string.Format("SELECT {1} FROM [{0}] {3} WHERE {2} {4} {5}", Mapper.TableName, GetSelect(), WhereStr.ToString(), GetJoin(), GetGroupBy(), GetOrderBy());
            }
        }

        private string GetPageSql()
        {
            if (PageIndex > 0 && PageSize > 0)
            {
                if (PageIndex == 1)
                {
                    return GetSql();
                }
                else
                {
                    if (Connection.State == ConnectionState.Open && Connection.ServerVersion != null && int.Parse(Connection.ServerVersion.Substring(0, 2)) > 10)
                    {
                        return string.Format("{0} {1} OFFSET {2} ROWS FETCH NEXT {3} ROWS ONLY", GetSql(), GetOrderBy(), (PageIndex - 1) * PageSize, PageSize);
                    }
                    return string.Format("SELECT * FROM ({0}) AS [ROW_TABLE] WHERE [ROW_NUM] BETWEEN {1} AND {2}", GetSql(), (PageIndex - 1) * PageSize + 1, PageIndex * PageSize);
                }
            }
            throw new Exception("PageIndex和PageSize必须大于0!");
        }

        internal override string GetCount()
        {
            return string.Format("SELECT {1} FROM [{0}] {3} WHERE {2} {4} {5}", Mapper.TableName, "COUNT(1)", WhereStr.ToString(), GetJoin(), GetGroupBy(), "");
        }


        internal override string GetSelect()
        {
            if (PageIndex > 0 && PageSize > 0)
            {
                if (PageIndex == 1)
                {
                    return string.Format(" TOP {0} {1}", PageSize.ToString(), SelectStr.ToString());
                }
                else
                {
                    if (Connection.State == ConnectionState.Open && Connection.ServerVersion != null &&
                        int.Parse(Connection.ServerVersion.Substring(0, 2)) > 10)
                    {
                        return SelectStr.ToString();
                    }
                    return string.Format(" {0},Row_Number() OVER({1}) AS [ROW_NUM]", SelectStr.ToString(), GetOrderBy());
                }
            }
            else
            {
                if (PageSize > -1)
                {
                    return string.Format(" TOP {0} {1}", PageSize.ToString(), SelectStr.ToString());
                }
                else
                {
                    return SelectStr.ToString();
                }
            }

        }

        internal override string GetJoin()
        {
            if (Mapper.ForeignKeyProperties.Count > 0)
            {
                var res = new StringBuilder();
                foreach (var tableKeyProperties in Mapper.ForeignKeyProperties)
                {
                    res.Append(string.Format(" LEFT JOIN [{0}] ON ", tableKeyProperties.Key));

                    foreach (var propertyMap in tableKeyProperties.Value)
                    {
                        res.Append(string.Format(" [{0}].[{2}] = [{1}].[{3}] AND", propertyMap.Value.TableName, Mapper.TableName, propertyMap.Value.ColumnName, propertyMap.Value.Name));
                    }
                    res.Remove(res.Length - 3, 3);
                }
                return res.ToString();
            }
            return "";
        }

        internal override string GetOrderBy()
        {
            if (PageIndex > 0 && PageSize > 0)
            {
                if (GroupByStr.Length > 0)
                    return OrderByStr.Length > 0 ?
                       string.Format(" ORDER BY {0}", OrderByStr.ToString().TrimEnd(','))
                       : "";
                else
                    return OrderByStr.Length > 0 ?
                        (
                        string.Format(" ORDER BY {0}", OrderByStr.ToString().TrimEnd(',') + "," +
                        Mapper.KeyProperties.Values.Where(propertyMap => !OrderByStr.ToString().Contains(propertyMap.Where)).Aggregate("", (current, propertyMap) => current + (propertyMap.Where + ",")).TrimEnd(','))
                        ).TrimEnd(',')
                        :
                        string.Format(" ORDER BY {0}", Mapper.KeyProperties.Values.Aggregate("", (current, propertyMap) => current + (propertyMap.Where + ",")).TrimEnd(','));
            }
            else
            {
                return OrderByStr.Length > 0 ? string.Format(" ORDER BY {0} ", OrderByStr.ToString().TrimEnd(',')) : "";
            }
        }

        internal override string GetGroupBy()
        {
            return GroupByStr.Length > 0 ? string.Format(" GROUP BY {0} ", GroupByStr.ToString()) : "";
        }

        internal override SelectQuery<T> Select(params Expression<Func<T, dynamic>>[] exprs)
        {
            if (exprs.Length > 0)
            {
                var p = new ParserArgs { Mapper = Mapper };
                foreach (var expr in exprs)
                {
                    if (expr.Body is MemberExpression | (expr.Body is UnaryExpression && ((UnaryExpression)expr.Body).Operand is MemberExpression))
                    {
                        LambdaParser.Select(expr.Body, p);
                    }
                    else
                    {
                        throw new Exception("非法的SELECT内容!");
                    }
                }
                SelectStr = p.Builder.Remove(p.Builder.Length - 1, 1);
            }
            else
            {
                SelectStr = new StringBuilder();
                foreach (var propertyMap in Mapper.SelectProperties.Values)
                {
                    SelectStr.Append(propertyMap.Select);
                    SelectStr.Append(",");
                }
                SelectStr = SelectStr.Remove(SelectStr.Length - 1, 1);
            }
            return this;
        }



        public override SelectQuery<T> Where(Expression<Func<T, bool>> expr = null)
        {
            if (expr != null)
            {
                var p = new ParserArgs { Mapper = Mapper };
                LambdaParser.Where(expr.Body, p);
                WhereStr = p.Builder;
                foreach (var item in p.Parameters)
                {
                    DapperParameters.Add(item.Key, item.Value);
                }
            }
            return this;
        }

        public override SelectQuery<T> OrderBy(params Expression<Func<T, OrderType>>[] exprs)
        {
            if (exprs != null && exprs.Length > 0)
            {
                var p = new ParserArgs { Mapper = Mapper };
                foreach (var expr in exprs)
                {
                    LambdaParser.OrderBy(expr.Body, p);
                }
                OrderByStr = p.Builder.Remove(p.Builder.Length - 1, 1);
            }
            return this;
        }

        public override SelectQuery<T> GroupBy(params Expression<Func<T, dynamic>>[] exprs)
        {
            if (exprs != null && exprs.Length > 0)
            {
                var p = new ParserArgs { Mapper = Mapper };
                foreach (var expr in exprs)
                {
                    LambdaParser.GroupBy(expr.Body, p);
                }
                GroupByStr = p.Builder.Remove(p.Builder.Length - 1, 1);
            }
            return this;
        }

        private void GetDebugSql(string sqlstr, DynamicParameters parameters)
        {
            string result = sqlstr;
            foreach (var name in parameters.ParameterNames)
            {
                var value = parameters.Get<object>(name);
                if (value.GetType().BaseType == typeof(Array))
                {
                    result += name + "_";
                    result = (value as Array).Cast<object>().Aggregate(result, (current, item) => current + item.ToString());
                }
                else
                {
                    result += (name + "_" + parameters.Get<object>(name).ToString());
                }
            }
            DebugSql = result;
        }


        public override List<T> ToList(long top = -1)
        {
            PageSize = top;

            if (IsDebug)
            {
                GetDebugSql(GetSql(), DapperParameters);
                return null;
            }

            return Connection.Query<T>(GetSql(), DapperParameters, Transaction).ToList();
        }

        public override T ToSingle()
        {
            PageSize = 1;

            if (IsDebug)
            {
                GetDebugSql(GetSql(), DapperParameters);
                return null;
            }

            return Connection.Query<T>(GetSql(), DapperParameters, Transaction).FirstOrDefault();
        }

        public override PageList<T> ToPageList(long pageIndex, int pageSize)
        {
            if (pageIndex <= 0)
                throw new Exception("PageIndex必须为大于0的整数!");
            if (pageSize <= 0)
                throw new Exception("PageSize必须为大于0的整数!");

            PageIndex = pageIndex;
            PageSize = pageSize;

            if (IsDebug)
            {
                GetDebugSql(GetPageSql(), DapperParameters);
                return null;
            }

            var res = new PageList<T>
            {
                TotalRowCount = ToLongCount(),
                Rows = Connection.Query<T>(GetPageSql(), DapperParameters, Transaction).ToList()
            };
            res.TotalPageCount = res.TotalRowCount / (long)pageSize;
            if (res.TotalPageCount * (long)pageSize < res.TotalRowCount)
            {
                res.TotalPageCount++;
            }
            res.PageIndex = pageIndex;
            res.PageSize = pageSize;
            return res;
        }

        public override int ToCount()
        {
            if (IsDebug)
            {
                GetDebugSql(GetCount(), DapperParameters);
                return 0;
            }

            return Connection.Query<int>(GetCount(), DapperParameters, Transaction).SingleOrDefault();
        }

        public override long ToLongCount()
        {
            if (IsDebug)
            {
                GetDebugSql(GetCount(), DapperParameters);
                return 0;
            }

            return Connection.Query<long>(GetCount(), DapperParameters, Transaction).SingleOrDefault();
        }
    }
}