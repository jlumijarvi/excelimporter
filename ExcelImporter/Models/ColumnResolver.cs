using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ExcelImporter.Models
{
    public class ColumnResolver
    {
        public IEnumerable<Type> Types { get; set; }

        public ColumnResolver(IEnumerable<Type> types)
        {
            Types = types;
        }

        public PropertyInfo Resolve(string fieldName)
        {
            var ret = default(PropertyInfo);

            foreach (var type in Types)
            {
                foreach (var pi in type.GetProperties())
                {
                    var propName = pi.Name;
                    if (string.Compare(pi.Name.ToLower(), "id", true) == 0)
                        propName = type.Name + pi.Name;

                    if (string.Compare(propName, fieldName, true) == 0)
                    {
                        return pi;
                    }
                    else if (string.Compare(type.Name + propName, fieldName, true) == 0)
                    {
                        return pi;
                    }
                }
            }

            return ret;
        }
    }
}
