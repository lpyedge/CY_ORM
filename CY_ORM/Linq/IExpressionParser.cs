using System.Linq.Expressions;

namespace CY_ORM.Linq
{
    /// <summary> 表达式树解析接口
    /// </summary>
    internal interface IExpressionParser
    {
        void InsertColumn(Expression expr, ParserArgs args);
        void InsertValue(Expression expr, ParserArgs args);
        void UpdateSet(Expression expr, ParserArgs args);
        void Select(Expression expr, ParserArgs args);
        void Where(Expression expr, ParserArgs args);
        void GroupBy(Expression expr, ParserArgs args);
        void Having(Expression expr, ParserArgs args);
        void OrderBy(Expression expr, ParserArgs args);
        void Object(Expression expr, ParserArgs args);
    }
}
