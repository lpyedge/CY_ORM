using System;

namespace CY_ORM
{
    public static class CommonExtends
    {
        public static bool IsNull(this object key)
        {
            throw new Exception("It's only for Linq");
        }

        public static bool IsNotNull(this object key)
        {
            throw new Exception("It's only for Linq");
        }


        //public static bool In<T>(this string key, List<string> values)
        //{
        //    throw new Exception("It's only for Linq");
        //}

        //public static bool In<T>(this T key, IEnumerable<T> values) where T : struct
        //{
        //    throw new Exception("It's only for Linq");
        //}


        public static bool In<T>(this T key, params T[] values) where T : struct
        {
            throw new Exception("It's only for Linq");
        }
        public static bool In(this string key, params string[] values)
        {
            throw new Exception("It's only for Linq");
        }
        public static bool InSql<T>(this T key, string sql) where T : struct
        {
            throw new Exception("It's only for Linq");
        }

        public static bool InSql(this string key, string sql) 
        {
            throw new Exception("It's only for Linq");
        }

        public static bool NotIn<T>(this T key, params T[] values) where T : struct
        {
            throw new Exception("It's only for Linq");
        }
        public static bool NotIn(this string key, params string[] values) 
        {
            throw new Exception("It's only for Linq");
        }

        public static bool NotInSql<T>(this T key, string sql) where T : struct
        {
            throw new Exception("It's only for Linq");
        }
        public static bool NotInSql(this string key, string sql) 
        {
            throw new Exception("It's only for Linq");
        }

        public static OrderType Order(this object key, OrderType orderType = OrderType.Asc)
        {
            throw new Exception("It's only for Linq");
        }
    }
}