using System;

namespace CY_ORM
{
    /// <summary>
    /// 列映射标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : System.Attribute
    {
        public ColumnAttribute()
        {
            TableName = "";
        }

        /// <summary>
        /// 对应表的名称
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 得到或设置列名
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///  是否为外键
        /// </summary>
        public bool IsKey { get; set; }
    }

}

