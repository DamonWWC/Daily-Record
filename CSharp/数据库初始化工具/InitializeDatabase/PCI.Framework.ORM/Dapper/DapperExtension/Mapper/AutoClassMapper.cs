using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace PCI.Framework.ORM.DapperExtensions.Mapper
{
    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys.
    /// </summary>
    public class AutoClassMapper<T> : ClassMapper<T> where T : class
    {
        /// <summary>
        /// constructor
        /// </summary>
        public AutoClassMapper()
        {
            Type type = typeof(T);
            Table(type.Name);
            AutoMap();
        }
    }
}