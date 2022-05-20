using System;
using System.Security.Policy;

namespace CY_ORM
{
    /// <summary>
    /// 主键映射标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class KeyAttribute : ColumnAttribute
    {
        public KeyAttribute()
        {
            Assigned = false;
            Ignored = false;
        }
        /// <summary>
        /// 对应列名
        /// </summary>
        public new string ColumnName { get; set; }

        /// <summary>
        /// 是否非程序设置主键
        /// </summary>
        public bool Assigned { get; set; }
    }

}

