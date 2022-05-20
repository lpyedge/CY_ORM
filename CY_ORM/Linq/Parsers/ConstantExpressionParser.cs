using System;
using System.Linq.Expressions;

namespace CY_ORM.Linq
{
    class ConstantExpressionParser : ExpressionParser<ConstantExpression>
    {
        public override void InsertColumn(ConstantExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void InsertValue(ConstantExpression expr, ParserArgs args)
        {
            args.AddParameter(expr.Value, "i");
        }

        public override void UpdateSet(ConstantExpression expr, ParserArgs args)
        {
            args.AddParameter(expr.Value, "u");
        }

        public override void Where(ConstantExpression expr, ParserArgs args)
        {
            args.Builder.Append(' ');
            args.AddParameter(expr.Value, "w");
        }

        public override void Select(ConstantExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }


        public override void GroupBy(ConstantExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void Having(ConstantExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void OrderBy(ConstantExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }

        public override void Object(ConstantExpression expr, ParserArgs args)
        {
            throw new Exception("未实现方法，内部错误！" + this);
        }
    }
}
