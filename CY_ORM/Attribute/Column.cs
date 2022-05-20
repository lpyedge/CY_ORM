using System;

namespace CY_ORM
{
    /// <summary>
    /// 列映射标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ColumnAttribute : System.Attribute
    {
        public ColumnAttribute()
        {
            Ignored = false;
        }
        /// <summary>
        /// 对应列名
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 是否忽略
        /// </summary>
        public bool Ignored { get; set; }
    }
}

