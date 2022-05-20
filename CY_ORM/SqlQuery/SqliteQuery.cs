using CY_ORM.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CY_ORM.SqlQuery
{
    public class SqliteInsertQuery<T> : InsertQuery<T> where T : class
    {
        public SqliteInsertQuery(DbConnection conn, DbTransaction tran = null)
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
                InsertColumnStr = p.Builder.Remove(p.Builder.Length - 1, 1).Replace("[" + Mapper.TableName + "].", "");
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
                InsertColumnStr = InsertColumnStr.Remove(InsertColumnStr.Length - 1, 1).Replace("[" + Mapper.TableName + "].", "");
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
                InsertColumnStr = p1.Builder.Remove(p1.Builder.Length - 1, 1).Replace("[" + Mapper.TableName + "].", "");
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
            return "SELECT LAST_INSERT_ROWID() AS [Id]";
        }

        internal override dynamic ExcuteList(IEnumerable<T> p_Objs)
        {
            var pList = p_Objs as T[] ?? p_Objs.ToArray();
            if (p_Objs != null && pList.Length > 0)
            {
                int index = 0;
                foreach (var propertyMap in Mapper.InsertProperties.Values)
                {
                    InsertColumnStr.Append(propertyMap.Select);
                    InsertColumnStr.Append(",");
                    InsertValueStr.Append(string.Format("@i{0},", index));
                    index++;
                }
                InsertColumnStr = InsertColumnStr.Remove(InsertColumnStr.Length - 1, 1).Replace("[" + Mapper.TableName + "].", "");
                InsertValueStr = InsertValueStr.Remove(InsertValueStr.Length - 1, 1);


                foreach (var obj in pList)
                {
                    var parameters = new DynamicParameters();
                    foreach (var propertyMap in Mapper.InsertProperties.Values)
                    {
                        parameters.Add(string.Format("i{0}", DapperParameters.ParameterNames.Count()),
                            Reflection.Property.GetValue(obj, propertyMap.Name));
                    }

                    Connection.Execute(GetSql(), parameters, Transaction);
                }
                
                var id = (IDictionary<string, object>)Connection.Query<CY_ORM.SqlMapper.DapperRow>(GetIdentitySql(), null, Transaction).FirstOrDefault();
                return (id != null && id.Count > 0) ? id.Values.FirstOrDefault() : null;
            }
            return null;
        }
    }

    public class SqliteDeleteQuery<T> : DeleteQuery<T> where T : class
    {
        public SqliteDeleteQuery(DbConnection conn, DbTransaction tran = null)
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

    public class SqliteUpdateQuery<T> : UpdateQuery<T> where T : class
    {
        public SqliteUpdateQuery(DbConnection conn, DbTransaction tran = null)
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
                SetStr = p.Builder.Remove(p.Builder.Length - 1, 1).Replace("[" + Mapper.TableName + "].", "");
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
                SetStr = p.Builder.Remove(p.Builder.Length - 1, 1).Replace("[" + Mapper.TableName + "].", "");
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
                SetStr = SetStr.Remove(SetStr.Length - 1, 1).Replace("[" + Mapper.TableName + "].", "");
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
            if (objs != null && pList.Any())
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
                    SetStr = p.Builder.Remove(p.Builder.Length - 1, 1).Replace("[" + Mapper.TableName + "].", "");
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
                    SetStr = SetStr.Remove(SetStr.Length - 1, 1).Replace("[" + Mapper.TableName + "].", "");
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

    public class SqliteSelectQuery<T> : SelectQuery<T> where T : class
    {
        public SqliteSelectQuery(DbConnection conn, DbTransaction tran = null)
            : base(conn, tran)
        {

        }

        internal override string GetSql()
        {
            if (PageIndex > 0 && PageSize > 0)
            {
                return string.Format("SELECT {1} FROM [{0}] {3} WHERE {2} {4} {5} LIMIT {6}", Mapper.TableName, GetSelect(), WhereStr.ToString(), GetJoin(), GetGroupBy(), GetOrderBy(), GetLimit());
            }
            else
            {
                return string.Format("SELECT {1} FROM [{0}] {3} WHERE {2} {4} {5}", Mapper.TableName, GetSelect(), WhereStr.ToString(), GetJoin(), GetGroupBy(), GetOrderBy());
            }
        }

        private string GetLimit()
        {
            if (PageIndex > 0 && PageSize > 0)
            {
                return string.Format("{0},{1}", (PageIndex - 1) * PageSize, PageSize);
            }
            throw new Exception("PageIndex和PageSize必须大于0!");
        }

        internal override string GetCount()
        {
            return string.Format("SELECT {1} FROM [{0}] {3} WHERE {2} {4} {5}", Mapper.TableName, "COUNT(1)", WhereStr.ToString(), GetJoin(), GetGroupBy(), "");
        }


        internal override string GetSelect()
        {
            return SelectStr.ToString();
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
                return OrderByStr.Length > 0 ? string.Format(" ORDER BY {0} ", OrderByStr.ToString().TrimEnd(',')) : string.Format(" ORDER BY {0}", Mapper.KeyProperties.Values.Aggregate("", (current, propertyMap) => current + (propertyMap.Where + ",")).TrimEnd(','));
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
            if (top != -1)
            {
                PageIndex = 1;
                PageSize = top;
            }

            if (IsDebug)
            {
                GetDebugSql(GetSql(), DapperParameters);
                return null;
            }

            return Connection.Query<T>(GetSql(), DapperParameters, Transaction).ToList();
        }

        public override T ToSingle()
        {
            PageIndex = 1;
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
                GetDebugSql(GetSql(), DapperParameters);
                return null;
            }

            var res = new PageList<T>
            {
                TotalRowCount = ToLongCount(),
                Rows = Connection.Query<T>(GetSql(), DapperParameters, Transaction).ToList()
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