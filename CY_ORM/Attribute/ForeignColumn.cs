using System;

namespace CY_ORM
{
    /// <summary>
    /// 列映射标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ForeignColumnAttribute : ColumnAttribute
    {
        public ForeignColumnAttribute(string p_TableName)
        {
            TableName = p_TableName;
            IsKey = false;
        }
        /// <summary>
        /// 对应列名
        /// </summary>
        public new string ColumnName { get; set; }
        /// <summary>
        /// 对应表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 是否联接外表主键
        /// </summary>
        public bool IsKey { get; set; }
    }
}

