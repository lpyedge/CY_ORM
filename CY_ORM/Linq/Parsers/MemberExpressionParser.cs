using System;
using System.Collections;
using System.Linq.Expressions;

namespace CY_ORM.Linq
{
    class MemberExpressionParser : ExpressionParser<MemberExpression>
    {
        public override void InsertColumn(MemberExpression expr, ParserArgs args)
        {
            if (expr.Expression is ParameterExpression)
            {
                args.Builder.Append(args.Mapper.SelectProperties[expr.Member.Name].Select);
            }
        }

        public override void InsertValue(MemberExpression expr, ParserArgs args)
        {
            args.AddParameter(LambdaParser.CompileInvoke(expr), "i");
        }

        public override void UpdateSet(MemberExpression expr, ParserArgs args)
        {
            if (expr.Expression is ParameterExpression)
            {
                args.Builder.Append(args.Mapper.SelectProperties[expr.Member.Name].Select);
            }
            else if (expr.Expression is MemberExpression)
            {
                args.AddParameter(LambdaParser.CompileInvoke(expr), "u");
            }
            else if (expr.Expression is ConstantExpression)
            {
                args.AddParameter(LambdaParser.CompileInvoke(expr), "u");
            }
            else if (expr.Expression == null)
            {
                args.AddParameter(LambdaParser.CompileInvoke(expr), "u");
            }
            else
            {
                throw new Exception("未实现方法，内部错误！" + this);
            }
        }

        public override void Where(MemberExpression expr, ParserArgs args)
        {

            if (expr.Expression is MemberExpression)
            {
                args.AddParameter(LambdaParser.CompileInvoke(expr), "w");
            }
            else if (expr.Expression is ParameterExpression)
            {
                args.Builder.Append(args.Mapper.SelectProperties[expr.Member.Name].Where);
            }
            else
            {
                object val = LambdaParser.CompileInvoke(expr);
                args.Builder.Append(' ');
                IEnumerator array = val as IEnumerator;
                if (array != null)
                {
                    AppendArray(args, array);
                }
                else if (!(val is string) && val is IEnumerable)
                {
                    AppendArray(args, ((IEnumerable)val).GetEnumerator());
                }
                else
                {
                    AppendObject(args, val, "w");
                }
            }
        }

        /// <summary> 追加可遍历对象(数组或集合或简单迭代器)
        /// </summary>
        private static void AppendArray(ParserArgs args, IEnumerator array)
        {
            if (array.MoveNext())
            {
                args.Builder.Append('(');
                AppendObject(args, array.Current, "w");
                while (array.MoveNext())
                {
                    args.Builder.Append(',');
                    AppendObject(args, array.Current, "w");
                }
                args.Builder.Append(')');
            }
            else
            {
                args.Builder.Append("NULL");
            }
        }

        /// <summary> 追加一般对象
        /// </summary>
        public static void AppendObject(ParserArgs args, object val, string prefix)
        {
            args.AddParameter(val, prefix);
        }


        public override void Select(MemberExpression expr, ParserArgs args)
        {
            if (expr.Expression is ParameterExpression)
            {
                args.Builder.Append(args.Mapper.SelectProperties[expr.Member.Name].Select);
                args.Builder.Append(',');
            }
        }


        public override void GroupBy(MemberExpression expr, ParserArgs args)
        {
            if (expr.Expression is ParameterExpression)
            {
                args.Builder.Append(args.Mapper.SelectProperties[expr.Member.Name].Where);
                args.Builder.Append(',');
            }
        }

        public override void Having(MemberExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void OrderBy(MemberExpression expr, ParserArgs args)
        {
            if (expr.Expression is ParameterExpression)
            {
                args.Builder.Append(args.Mapper.SelectProperties[expr.Member.Name].Where);
            }
        }

        public override void Object(MemberExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }
    }
}
