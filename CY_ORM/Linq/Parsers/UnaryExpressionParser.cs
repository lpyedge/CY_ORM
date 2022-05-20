using System;
using System.Linq.Expressions;

namespace CY_ORM.Linq
{
    class UnaryExpressionParser : ExpressionParser<UnaryExpression>
    {
        public override void InsertColumn(UnaryExpression expr, ParserArgs args)
        {
            if (expr.Operand is MemberExpression)
            {
                LambdaParser.InsertColumn(expr.Operand, args);
            }
        }

        public override void InsertValue(UnaryExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void UpdateSet(UnaryExpression expr, ParserArgs args)
        {
            if (expr.Operand is MemberExpression)
            {
                LambdaParser.UpdateSet(expr.Operand, args);
            }
        }

        public override void Select(UnaryExpression expr, ParserArgs args)
        {
            if (expr.Operand is MemberExpression)
            {
                LambdaParser.Select(expr.Operand, args);
            }
        }

        public override void Where(UnaryExpression expr, ParserArgs args)
        {
            if (expr.Operand is MemberExpression)
            {
                if ((expr.Operand as MemberExpression).Expression is MemberExpression)
                {
                    if (
                        ((expr.Operand as MemberExpression).Expression as MemberExpression).Expression is
                            ConstantExpression)
                    {
                        args.AddParameter(LambdaParser.CompileInvoke(expr.Operand as MemberExpression), "w");
                    }
                    else if (
                        ((expr.Operand as MemberExpression).Expression as MemberExpression).Expression is
                            MethodCallExpression)
                    {
                        LambdaParser.Where(expr.Operand as MemberExpression, args);
                    }
                    else
                    {
                        args.Builder.Append(args.Mapper.SelectProperties[(expr.Operand as MemberExpression).Member.Name].Where);
                    }
                }
                else
                {
                    if ((expr.Operand as MemberExpression).Expression is ConstantExpression)
                    {
                        args.AddParameter(LambdaParser.CompileInvoke(expr.Operand as MemberExpression), "w");
                    }
                    else if ((expr.Operand as MemberExpression).Expression is MethodCallExpression)
                    {
                        LambdaParser.Where(expr.Operand as MemberExpression, args);
                    }
                    else
                    {
                        args.Builder.Append(args.Mapper.SelectProperties[(expr.Operand as MemberExpression).Member.Name].Where);
                    }
                }
            }
            else
            {
                throw new Exception("未实现方法，内部错误！" + this);
            }
        }

        public override void GroupBy(UnaryExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void Having(UnaryExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void OrderBy(UnaryExpression expr, ParserArgs args)
        {
            LambdaParser.OrderBy(expr.Operand, args);
        }

        public override void Object(UnaryExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }
    }
}
