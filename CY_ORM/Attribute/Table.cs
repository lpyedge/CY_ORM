using System;

namespace CY_ORM
{
    /// <summary>
    /// 表映射标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : System.Attribute
    {
        public TableAttribute(string p_TableName)
        {
            TableName = p_TableName;
        }

        /// <summary>
        /// 得到或设置数据库表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 得到或设置数据库表构架名
        /// </summary>
        public string SchemaName { get; set; }
    }
}

