using CY_ORM.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CY_ORM.Linq
{
    internal class ParserArgs
    {
        public ParserArgs()
        {
            Builder = new StringBuilder();
            Parameters = new Dictionary<string, object>();
        }

        public ClassMapper Mapper { get; set; }

        public Dictionary<string, object> Parameters { get; set; }

        public StringBuilder Builder { get; private set; }

        /// <summary> 追加参数
        /// </summary>
        public void AddParameter(object obj, string prefix)
        {
            if (obj == null || obj == DBNull.Value)
            {
                Builder.Append("NULL");
            }
            else
            {
                string name = prefix + Parameters.Count;
                Parameters.Add(name, obj);
                Builder.Append('@');
                Builder.Append(name);
            }
        }
    }
}
