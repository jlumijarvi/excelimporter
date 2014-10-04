using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ExcelImporter.Models
{
    public class PropertyResolver
    {
        public IEnumerable<Type> Types { get; set; }
        public IEnumerable<HeaderPropertyMapping> Mappings { get; set; }

        public PropertyResolver(IEnumerable<Type> types)
        {
            Types = types;

            // initialize the mappings
            var mappings = default(List<HeaderPropertyMapping>);
            using (var db = new RegistryContext())
            {
                mappings = db.HeaderPropertyMapping.ToList();
            }

            foreach (var type in Types)
            {
                foreach (var pi in type.GetProperties())
                {
                    var propName = pi.Name;
                    if (string.Compare(pi.Name, "id", true) == 0)
                        propName = type.Name + pi.Name;

                    mappings.Add(new HeaderPropertyMapping()
                    {
                        Type = type.FullName,
                        Property = pi.Name,
                        Header = propName
                    });

                    mappings.Add(new HeaderPropertyMapping()
                    {
                        Type = type.FullName,
                        Property = pi.Name,
                        Header = type.Name + propName
                    });
                }
            }

            Mappings = mappings;
        }

        /// <summary>
        /// Resolve the property from the given header based on the given types and mappings stored to db.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public PropertyInfo Resolve(string header)
        {
            var hcm = Mappings.Where(it => 
                header == it.Header && 
                !string.IsNullOrEmpty(it.Type) && 
                !string.IsNullOrEmpty(it.Property)).FirstOrDefault();

            if (hcm != null)
            {
                var t = Type.GetType(hcm.Type);
                if (t != null)
                {
                    return t.GetProperty(hcm.Property);
                }
            }

            return null;
        }
    }
}
