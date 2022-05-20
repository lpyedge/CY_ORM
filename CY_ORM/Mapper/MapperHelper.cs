using System;
using System.Collections.Concurrent;
using System.Numerics;

namespace CY_ORM.Mapper
{
    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys.
    /// </summary>
    internal static class MapperHelper
    {
        private static ConcurrentDictionary<string, ClassMapper> ListMappers = new ConcurrentDictionary<string, ClassMapper>();
        
        public static ClassMapper Get<T>() where T : class
        {
            var type = typeof(T);
            string name = type.FullName;
            if (!ListMappers.ContainsKey(name))
            {
                ListMappers[name] = new ClassMapper(type);
            }
            return ListMappers[name];
        }
    }
}