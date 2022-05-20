using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CY_ORM.Mapper
{
    internal class ClassMapper
    {
        public string SchemaName { get; protected set; }

        public string TableName { get; protected set; }

        public Type EntityType { get; protected set; }

        /// <summary>
        /// Select的属性列
        /// </summary>
        public ConcurrentDictionary<string, PropertyMap> SelectProperties { get; protected set; }
        /// <summary>
        /// Insert的属性列
        /// </summary>
        public ConcurrentDictionary<string, PropertyMap> InsertProperties { get; protected set; }
        /// <summary>
        /// Update的属性列
        /// </summary>
        public ConcurrentDictionary<string, PropertyMap> UpdateProperties { get; protected set; }
        /// <summary>
        /// Key主键的属性列
        /// </summary>
        public ConcurrentDictionary<string, PropertyMap> KeyProperties { get; protected set; }

        /// <summary>
        /// 联表表名及对应外键属性列
        /// </summary>
        public ConcurrentDictionary<string, ConcurrentDictionary<string, PropertyMap>> ForeignKeyProperties { get; protected set; }

        public ClassMapper(Type type)
        {
            EntityType = type;

            SelectProperties = new ConcurrentDictionary<string, PropertyMap>();
            InsertProperties = new ConcurrentDictionary<string, PropertyMap>();
            UpdateProperties = new ConcurrentDictionary<string, PropertyMap>();
            KeyProperties = new ConcurrentDictionary<string, PropertyMap>();

            ForeignKeyProperties = new ConcurrentDictionary<string, ConcurrentDictionary<string, PropertyMap>>();

            AutoMap();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void AutoMap()
        {
            TableAttribute tableAttribute = Reflection.Attribute.GetCustomAttribute<TableAttribute>(EntityType).FirstOrDefault();
            if (tableAttribute != null)
            {
                SchemaName = tableAttribute.SchemaName ?? "";
                TableName = tableAttribute.TableName ?? EntityType.Name;
            }
            else
            {
                SchemaName = "";
                TableName = EntityType.Name;
            }

            foreach (var propertyInfo in EntityType.GetProperties().Where(p => p.CanWrite && p.CanRead))
            {

                ColumnAttribute columnAttribute = Reflection.Attribute.GetCustomAttribute<ColumnAttribute>(propertyInfo).FirstOrDefault(); ;

                //列名特性忽略当前的话直接跳到下一个属性
                if (columnAttribute != null && columnAttribute.Ignored)
                {
                    continue;
                }
                KeyAttribute keyAttribute = Reflection.Attribute.GetCustomAttribute<KeyAttribute>(propertyInfo).FirstOrDefault(); ;
                List<ForeignColumnAttribute> foreignColumnAttribute = Reflection.Attribute.GetCustomAttribute<ForeignColumnAttribute>(propertyInfo);


                //判断当前属性是否有对应的主键特性，对应设置
                if (keyAttribute != null)
                {
                    PropertyMap pm = new PropertyMap(propertyInfo, TableName, (keyAttribute.ColumnName ?? propertyInfo.Name), propertyInfo.Name);

                    SelectProperties[pm.Name] = pm;
                    KeyProperties[pm.Name] = pm;

                    //主键非自增列,Insert和Update时需要操作
                    if (keyAttribute.Assigned)
                    {
                        InsertProperties[pm.Name] = pm;
                        UpdateProperties[pm.Name] = pm;
                    }
                }

                //判断当前属性是否有对应的外表特性，对应设置
                if (foreignColumnAttribute.Count > 0)
                {
                    foreach (var item in foreignColumnAttribute)
                    {
                        if (!ForeignKeyProperties.ContainsKey(item.TableName))
                            ForeignKeyProperties[item.TableName] = new ConcurrentDictionary<string, PropertyMap>();
                        PropertyMap pm = new PropertyMap(propertyInfo, item.TableName, (item.ColumnName ?? propertyInfo.Name), (columnAttribute != null ? (columnAttribute.ColumnName ?? propertyInfo.Name) : propertyInfo.Name));

                        if (!item.IsKey)
                        {
                            SelectProperties[pm.Name] = pm;
                        }
                        else
                        {
                            ForeignKeyProperties[item.TableName][pm.Name] = pm;

                            string columnName = propertyInfo.Name;
                            if (columnAttribute != null)
                                columnName = (columnAttribute.ColumnName ?? propertyInfo.Name);
                            pm = new PropertyMap(propertyInfo, TableName, columnName, propertyInfo.Name);

                            SelectProperties[pm.Name] = pm;

                            //主键特性不存在或者主键非自增列,Insert和Update时需要操作
                            if (keyAttribute == null || keyAttribute.Assigned)
                            {
                                UpdateProperties[pm.Name] = pm;
                                InsertProperties[pm.Name] = pm;
                            }
                        }
                    }
                }
                else
                {
                    string columnName = propertyInfo.Name;
                    if (columnAttribute != null)
                        columnName = (columnAttribute.ColumnName ?? propertyInfo.Name);
                    PropertyMap pm = new PropertyMap(propertyInfo, TableName, columnName, propertyInfo.Name);

                    SelectProperties[pm.Name] = pm;

                    //主键特性不存在或者主键非自增列,Insert和Update时需要操作
                    if (keyAttribute == null || keyAttribute.Assigned)
                    {
                        UpdateProperties[pm.Name] = pm;
                        InsertProperties[pm.Name] = pm;
                    }
                }
            }


            if (KeyProperties.Count == 0)
            {
                var name = EntityType.GetProperties().Select(p=>p.Name).FirstOrDefault(p => string.Equals(p, "id", StringComparison.InvariantCultureIgnoreCase));
                if (name == null)
                    name = EntityType.GetProperties().Select(p => p.Name).FirstOrDefault(p => p.EndsWith("id", StringComparison.InvariantCultureIgnoreCase));
                if (name == null)
                    name = EntityType.GetProperties().Select(p => p.Name).FirstOrDefault(p => string.Equals(p, "name", StringComparison.InvariantCultureIgnoreCase));
                if (name == null)
                    name = EntityType.GetProperties().Select(p => p.Name).FirstOrDefault(p => p.EndsWith("name", StringComparison.InvariantCultureIgnoreCase));
                if (name == null)
                    name = EntityType.GetProperties().Select(p => p.Name).FirstOrDefault();

                PropertyMap pm = SelectProperties[name];
                KeyProperties[name] = pm;
                //如果是自增列数据类型,从对应的Insert和Update属性列中删除
                if (pm.PropertyInfo.PropertyType != typeof(string))
                {
                    InsertProperties.TryRemove(name, out pm);
                    UpdateProperties.TryRemove(name, out pm);
                }
            }
        }
    }
}