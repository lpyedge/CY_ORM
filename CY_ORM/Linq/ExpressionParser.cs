using System.Linq.Expressions;

namespace CY_ORM.Linq
{
    /// <summary> 表达式树解析抽象泛型类
    /// </summary>
    internal abstract class ExpressionParser<T> : IExpressionParser
        where T : Expression
    {
        public abstract void InsertColumn(T expr, ParserArgs args);
        public abstract void InsertValue(T expr, ParserArgs args);
        public abstract void UpdateSet(T expr, ParserArgs args);
        public abstract void Select(T expr, ParserArgs args);
        public abstract void Where(T expr, ParserArgs args);
        public abstract void GroupBy(T expr, ParserArgs args);
        public abstract void Having(T expr, ParserArgs args);
        public abstract void OrderBy(T expr, ParserArgs args);
        public abstract void Object(T expr, ParserArgs args);

        public void InsertColumn(Expression expr, ParserArgs args)
        {
            InsertColumn((T)expr, args);
        }

        public void InsertValue(Expression expr, ParserArgs args)
        {
            InsertValue((T)expr, args);
        }

        public void UpdateSet(Expression expr, ParserArgs args)
        {
            UpdateSet((T)expr, args);
        }

        public void Select(Expression expr, ParserArgs args)
        {
            Select((T)expr, args);
        }

        public void Where(Expression expr, ParserArgs args)
        {
            Where((T)expr, args);
        }

        public void GroupBy(Expression expr, ParserArgs args)
        {
            GroupBy((T)expr, args);
        }

        public void Having(Expression expr, ParserArgs args)
        {
            Having((T)expr, args);
        }

        public void OrderBy(Expression expr, ParserArgs args)
        {
            OrderBy((T)expr, args);
        }

        public void Object(Expression expr, ParserArgs args)
        {
            Object((T)expr, args);
        }
    }
}
