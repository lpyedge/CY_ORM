using System;
using System.Linq;
using System.Reflection;

namespace CY_ORM.Mapper
{
    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
    internal class PropertyMap
    {
        public string Name { get; protected set; }
        public string TableName { get; protected set; }
        public string ColumnName { get; protected set; }
        public string ShowName { get; protected set; }
        public PropertyInfo PropertyInfo { get; set; }

        public PropertyMap(PropertyInfo p_PropertyInfo,string p_TableName, string p_ColumnName, string p_ShowName)
        {
            PropertyInfo = p_PropertyInfo;
            Name = p_PropertyInfo.Name;
            TableName = p_TableName;
            ColumnName = p_ColumnName;
            ShowName = p_ShowName;
        }

        public string Select
        {
            get
            {
                if (string.Equals(ShowName, ColumnName, StringComparison.OrdinalIgnoreCase))
                    return string.Format("[{0}].[{1}]", TableName, ShowName);
                else
                    return string.Format("[{0}].[{1}] AS [{2}]", TableName, ColumnName, ShowName);
            }
        }

        public string Where
        {
            get
            {
                return string.Format("[{0}].[{1}]", TableName, ShowName);
            }
        }
    }
}