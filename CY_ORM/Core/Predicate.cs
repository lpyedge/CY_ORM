using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CY_ORM
{
    public static class Predicate
    {        
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
         Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null)
            {
                return expr2;
            }
            if (expr2 == null)
            {
                return expr1;
            }
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
            (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);

        }

        public static Expression<Func<T, bool>> OrBracket<T>(this Expression<Func<T, bool>> expr1,
         Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null)
            {
                return expr2;
            }
            if (expr2 == null)
            {
                return expr1;
            }
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
            (Expression.Or(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
         Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null)
            {
                return expr2;
            }
            if (expr2 == null)
            {
                return expr1;
            }
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> AndBracket<T>(this Expression<Func<T, bool>> expr1,
         Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null)
            {
                return expr2;
            }
            if (expr2 == null)
            {
                return expr1;
            }
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.And(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> Generate<T>(Expression<Func<T, bool>> expr)
        {
            return expr;
        }

        public static Expression<Func<T, bool>> Empty<T>()
        {
            return null;
        }
    }
}
