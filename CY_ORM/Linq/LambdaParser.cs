using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace CY_ORM.Linq
{
    /// <summary> 解析器静态对象 
    /// </summary>
    internal static class LambdaParser
    {
        private static readonly IExpressionParser[] Parsers = InitParsers();

        static IExpressionParser[] InitParsers()
        {
            var codes = Enum.GetValues(typeof(ExpressionTypeCode));
            var parsers = new IExpressionParser[codes.Length];

            foreach (ExpressionTypeCode code in codes)
            {
                if (code.ToString().EndsWith("Expression"))
                {
                    var type = Type.GetType(typeof(LambdaParser).Namespace + "." + code.ToString() + "Parser");
                    if (type != null)
                    {
                        parsers[(int)code] = (IExpressionParser)Activator.CreateInstance(type);
                    }
                }
            }
            return parsers;
        }

        /// <summary> 得到表达式类型的枚举对象 </summary>
        /// <param name="expr"> 扩展对象:Expression </param>
        /// <returns> </returns>
        public static ExpressionTypeCode GetCodeType(Expression expr)
        {
            if (expr == null)
            {
                return ExpressionTypeCode.Null;
            }
            if (expr is BinaryExpression)
            {
                return ExpressionTypeCode.BinaryExpression;
            }
            if (expr is BlockExpression)
            {
                return ExpressionTypeCode.BlockExpression;
            }
            if (expr is ConditionalExpression)
            {
                return ExpressionTypeCode.ConditionalExpression;
            }
            if (expr is ConstantExpression)
            {
                return ExpressionTypeCode.ConstantExpression;
            }
            if (expr is DebugInfoExpression)
            {
                return ExpressionTypeCode.DebugInfoExpression;
            }
            if (expr is DefaultExpression)
            {
                return ExpressionTypeCode.DefaultExpression;
            }
            if (expr is DynamicExpression)
            {
                return ExpressionTypeCode.DynamicExpression;
            }
            if (expr is GotoExpression)
            {
                return ExpressionTypeCode.GotoExpression;
            }
            if (expr is IndexExpression)
            {
                return ExpressionTypeCode.IndexExpression;
            }
            if (expr is InvocationExpression)
            {
                return ExpressionTypeCode.InvocationExpression;
            }
            if (expr is LabelExpression)
            {
                return ExpressionTypeCode.LabelExpression;
            }
            if (expr is LambdaExpression)
            {
                return ExpressionTypeCode.LambdaExpression;
            }
            if (expr is ListInitExpression)
            {
                return ExpressionTypeCode.ListInitExpression;
            }
            if (expr is LoopExpression)
            {
                return ExpressionTypeCode.LoopExpression;
            }
            if (expr is MemberExpression)
            {
                return ExpressionTypeCode.MemberExpression;
            }
            if (expr is MemberInitExpression)
            {
                return ExpressionTypeCode.MemberInitExpression;
            }
            if (expr is MethodCallExpression)
            {
                return ExpressionTypeCode.MethodCallExpression;
            }
            if (expr is NewArrayExpression)
            {
                return ExpressionTypeCode.NewArrayExpression;
            }
            if (expr is NewExpression)
            {
                return ExpressionTypeCode.NewArrayExpression;
            }
            if (expr is ParameterExpression)
            {
                return ExpressionTypeCode.ParameterExpression;
            }
            if (expr is RuntimeVariablesExpression)
            {
                return ExpressionTypeCode.RuntimeVariablesExpression;
            }
            if (expr is SwitchExpression)
            {
                return ExpressionTypeCode.SwitchExpression;
            }
            if (expr is TryExpression)
            {
                return ExpressionTypeCode.TryExpression;
            }
            if (expr is TypeBinaryExpression)
            {
                return ExpressionTypeCode.TypeBinaryExpression;
            }
            if (expr is UnaryExpression)
            {
                return ExpressionTypeCode.UnaryExpression;
            }
            return ExpressionTypeCode.Unknown;
        }

        /// <summary> 得到当前表达式对象的解析组件 </summary>
        /// <param name="expr"> 扩展对象:Expression </param>
        /// <returns> </returns>
        internal static IExpressionParser GetParser(Expression expr)
        {
            var codetype = GetCodeType(expr);
            var parser = Parsers[(int)codetype];
            if (parser == null)
            {
                switch (codetype)
                {
                    case ExpressionTypeCode.Unknown:
                        throw new ArgumentException("未知的表达式类型", "expr");
                    case ExpressionTypeCode.Null:
                        throw new ArgumentNullException("expr", "表达式为空");
                    default:
                        throw new NotImplementedException("尚未实现" + codetype + "的解析");
                }
            }
            return parser;
        }

        /// <summary>
        /// 计算Lmabda方法的返回值
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static dynamic CompileInvoke(Expression expr)
        {
            //object res = null;
            //int count = 5000;
            //Stopwatch sw1 = new Stopwatch(), sw2 = new Stopwatch();
            //sw1.Start();
            //for (int i = 0; i < count; i++)
            //{
            //    if (expr is ConstantExpression)
            //    {
            //        res = ((ConstantExpression)expr).Value;
            //    }
            //    else if (expr is MemberExpression)
            //    {
            //        try
            //        {
            //            res = Expression.Lambda(expr).Compile().DynamicInvoke();
            //        }
            //        catch (Exception)
            //        {
            //            res = null;
            //        }
            //    }
            //}

            //sw1.Stop();

            //sw2.Start();
            //for (int i = 0; i < count; i++)
            //{
            //    if (expr is ConstantExpression)
            //    {
            //        res = ((ConstantExpression)expr).Value;
            //    }
            //    else if (expr is MemberExpression)
            //    {
            //        if (((MemberExpression) expr).Expression != null)
            //        {
            //            var field = ((MemberExpression) expr).Member as FieldInfo;
            //            if (field != null)
            //            {
            //                if (((MemberExpression) expr).Expression is MemberExpression)
            //                {
            //                    var res1 = CompileInvoke(((MemberExpression) expr).Expression);
            //                    res = res1 != null ? field.GetValue(res1) : null;
            //                }
            //                else
            //                {
            //                    res = field.GetValue(((ConstantExpression) ((MemberExpression) expr).Expression).Value);
            //                }
            //            }
            //            else
            //            {
            //                if (((MemberExpression) expr).Expression is MemberExpression)
            //                {
            //                    var res1 = CompileInvoke(((MemberExpression) expr).Expression);
            //                    res = res1 != null
            //                        ? ((PropertyInfo) ((MemberExpression) expr).Member).GetValue(res1, null)
            //                        : null;
            //                }
            //                else
            //                {
            //                    res =
            //                        ((PropertyInfo) ((MemberExpression) expr).Member).GetValue(
            //                            ((ConstantExpression) ((MemberExpression) expr).Expression).Value, null);
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        try
            //        {
            //            res = Expression.Lambda(expr).Compile().DynamicInvoke();
            //        }
            //        catch (Exception)
            //        {
            //            res = null;
            //        }
            //    }
            //}
            //sw2.Stop();

            //return res;

            if (expr is ConstantExpression)
            {
                return ((ConstantExpression)expr).Value;
            }
            else if (expr is MemberExpression)
            {
                if (((MemberExpression)expr).Expression != null)
                {
                    var field = ((MemberExpression)expr).Member as FieldInfo;
                    if (field != null)
                    {
                        if (((MemberExpression)expr).Expression is MemberExpression)
                        {
                            var res = CompileInvoke(((MemberExpression)expr).Expression);
                            return res != null ? field.GetValue(res) : null;
                        }
                        else
                        {
                            return field.GetValue(((ConstantExpression)((MemberExpression)expr).Expression).Value);
                        }
                    }
                    else
                    {
                        if (((MemberExpression)expr).Expression is MemberExpression)
                        {
                            var res = CompileInvoke(((MemberExpression)expr).Expression);
                            return res != null ? ((PropertyInfo)((MemberExpression)expr).Member).GetValue(res, null) : null;
                        }
                        else
                        {
                            return ((PropertyInfo)((MemberExpression)expr).Member).GetValue(((ConstantExpression)((MemberExpression)expr).Expression).Value, null);
                        }
                    }
                }
            }
            try
            {
                return Expression.Lambda(expr).Compile().DynamicInvoke();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string MemberName(LambdaExpression expr)
        {
            if (expr.Body is MemberExpression)
            {
                return (expr.Body as MemberExpression).Member.Name;
            }
            if (expr.Body is UnaryExpression)
            {
                var operand = (expr.Body as UnaryExpression).Operand;
                if (operand is MemberExpression)
                {
                    return (operand as MemberExpression).Member.Name;
                }
            }
            return null;
        }

        ///// <summary> 获取成员表达式中的实际值
        ///// </summary>
        //private static object GetValue(MemberExpression expr)
        //{
        //    object val;
        //    var field = expr.Member as FieldInfo;
        //    if (field != null)
        //    {
        //        val = field.GetValue(((ConstantExpression)expr.Expression).Value);
        //    }
        //    else
        //    {
        //        val = ((PropertyInfo)expr.Member).GetValue(((ConstantExpression)expr.Expression).Value, null);
        //    }
        //    return val;
        //}

        public static void InsertColumn(Expression expr, ParserArgs args)
        {
            if (expr != null)
            {
                GetParser(expr).InsertColumn(expr, args);
            }
        }

        public static void InsertValue(Expression expr, ParserArgs args)
        {
            if (expr != null)
            {
                GetParser(expr).InsertValue(expr, args);
            }
        }

        public static void UpdateSet(Expression expr, ParserArgs args)
        {
            if (expr != null)
            {
                GetParser(expr).UpdateSet(expr, args);
            }
        }

        public static void Select(Expression expr, ParserArgs args)
        {
            if (expr != null)
            {
                GetParser(expr).Select(expr, args);
            }
        }

        public static void Where(Expression expr, ParserArgs args)
        {
            if (expr != null)
            {
                GetParser(expr).Where(expr, args);
            }
        }

        public static void GroupBy(Expression expr, ParserArgs args)
        {
            if (expr != null)
            {
                GetParser(expr).GroupBy(expr, args);
            }
        }

        public static void Having(Expression expr, ParserArgs args)
        {
            if (expr != null)
            {
                GetParser(expr).Having(expr, args);
            }
        }

        public static void OrderBy(Expression expr, ParserArgs args)
        {
            if (expr != null)
            {
                GetParser(expr).OrderBy(expr, args);
            }
        }

        public static void Object(Expression expr, ParserArgs args)
        {
            if (expr != null)
            {
                GetParser(expr).Object(expr, args);
            }
        }
    }
}