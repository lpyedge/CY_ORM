using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace CY_ORM.Linq
{
    class MethodCallExpressionParser : ExpressionParser<MethodCallExpression>
    {
        static ConcurrentDictionary<MethodInfo, Action<MethodCallExpression, ParserArgs>> _Methods = MethodDitcInit();

        private static ConcurrentDictionary<MethodInfo, Action<MethodCallExpression, ParserArgs>> MethodDitcInit()
        {
            ConcurrentDictionary<MethodInfo, Action<MethodCallExpression, ParserArgs>> dict = new ConcurrentDictionary<MethodInfo, Action<MethodCallExpression, ParserArgs>>();
            var type = typeof(string);
            foreach (var met in type.GetMethods())
            {
                switch (met.Name)
                {
                    case "StartsWith":
                        dict[met] = String_StartsWith;
                        break;
                    case "Contains":
                        dict[met] = String_Contains;
                        break;
                    case "EndsWith":
                        dict[met] = String_EndsWith;
                        break;
                    default:
                        break;
                }
            }
            foreach (var met in typeof(CommonExtends).GetMethods())
            {
                switch (met.Name)
                {
                    case "In":
                        dict[met] = Enumerable_In;
                        break;
                    case "InSql":
                        dict[met] = Enumerable_InSql;
                        break;
                    case "NotIn":
                        dict[met] = Enumerable_NotIn;
                        break;
                    case "NotInSql":
                        dict[met] = Enumerable_NotInSql;
                        break;
                    case "IsNull":
                        dict[met] = Column_IsNull;
                        break;
                    case "IsNotNull":
                        dict[met] = Column_IsNotNull;
                        break;
                    default:
                        break;
                }

            }
            return dict;
        }

        public override void Where(MethodCallExpression expr, ParserArgs args)
        {
            Action<MethodCallExpression, ParserArgs> act;
            var key = expr.Method;
            if (key.IsGenericMethod)
            {
                key = key.GetGenericMethodDefinition();
            }
            if (_Methods.TryGetValue(key, out act))
            {
                act(expr, args);
                return;
            }
            try
            {
                args.AddParameter(LambdaParser.CompileInvoke(expr), "w");
            }
            catch (Exception ex)
            {
                throw new NotImplementedException("无法解释方法" + expr.Method + ex.Message);
            }
        }

        public static void Enumerable_In(MethodCallExpression expr, ParserArgs args)
        {
            LambdaParser.Where(expr.Arguments[0], args);
            args.Builder.Append(" IN");
            LambdaParser.Where(expr.Arguments[1], args);
        }
        public static void Enumerable_InSql(MethodCallExpression expr, ParserArgs args)
        {
            LambdaParser.Where(expr.Arguments[0], args);
            args.Builder.Append(" IN (" + LambdaParser.CompileInvoke(expr.Arguments[1]) + ")");
        }
        public static void Enumerable_NotIn(MethodCallExpression expr, ParserArgs args)
        {
            LambdaParser.Where(expr.Arguments[0], args);
            args.Builder.Append(" NOT IN");
            LambdaParser.Where(expr.Arguments[1], args);
        }
        public static void Enumerable_NotInSql(MethodCallExpression expr, ParserArgs args)
        {
            LambdaParser.Where(expr.Arguments[0], args);
            args.Builder.Append(" NOT IN (" + LambdaParser.CompileInvoke(expr.Arguments[1]) + ")");
        }
        public static void Column_IsNull(MethodCallExpression expr, ParserArgs args)
        {
            LambdaParser.Where(expr.Arguments[0], args);
            args.Builder.Append(" IS NULL");
        }

        public static void Column_IsNotNull(MethodCallExpression expr, ParserArgs args)
        {
            LambdaParser.Where(expr.Arguments[0], args);
            args.Builder.Append(" IS NOT NULL");
        }

        public static void String_StartsWith(MethodCallExpression expr, ParserArgs args)
        {
            LambdaParser.Where(expr.Object, args);
            args.Builder.Append(" LIKE ");
            args.AddParameter(LambdaParser.CompileInvoke(expr.Arguments[0]) + "%", "w");
        }

        public static void String_Contains(MethodCallExpression expr, ParserArgs args)
        {
            LambdaParser.Where(expr.Object, args);
            args.Builder.Append(" LIKE ");
            args.AddParameter("%" + LambdaParser.CompileInvoke(expr.Arguments[0]) + "%", "w");
        }

        public static void String_EndsWith(MethodCallExpression expr, ParserArgs args)
        {
            LambdaParser.Where(expr.Object, args);
            args.Builder.Append(" LIKE ");
            args.AddParameter("%" + LambdaParser.CompileInvoke(expr.Arguments[0]), "w");
        }

        public override void InsertColumn(MethodCallExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void InsertValue(MethodCallExpression expr, ParserArgs args)
        {
            args.AddParameter(LambdaParser.CompileInvoke(expr), "i");
        }

        public override void UpdateSet(MethodCallExpression expr, ParserArgs args)
        {
            args.AddParameter(LambdaParser.CompileInvoke(expr), "u");
        }

        public override void Select(MethodCallExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }


        public override void GroupBy(MethodCallExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void Having(MethodCallExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void OrderBy(MethodCallExpression expr, ParserArgs args)
        {
            Expression ordernameExpr = expr.Arguments[0];
            if (expr.Arguments[0] is UnaryExpression)
            {
                ordernameExpr = (expr.Arguments[0] as UnaryExpression).Operand;
            }
            if (string.Equals(expr.Method.Name, "Order", StringComparison.OrdinalIgnoreCase))
            {
                if (expr.Arguments.Count == 2 && expr.Arguments[1] is ConstantExpression)
                {
                    switch ((OrderType)(expr.Arguments[1] as ConstantExpression).Value)
                    {
                        case OrderType.Desc:
                            LambdaParser.OrderBy(ordernameExpr, args);
                            args.Builder.Append(" Desc,");
                            break;
                        case OrderType.Asc:
                            LambdaParser.OrderBy(ordernameExpr, args);
                            args.Builder.Append(" Asc,");
                            break;
                        case OrderType.Count:
                            args.Builder.Append("COUNT(");
                            LambdaParser.OrderBy(ordernameExpr, args);
                            args.Builder.Append(")");
                            break;
                    }
                }
            }
        }

        public override void Object(MethodCallExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }
    }
}
