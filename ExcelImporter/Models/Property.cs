using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ExcelImporter.Models
{
    public class Property
    {
        public string Name { get; set; }
        public bool Key { get; set; }
        public bool Required { get; set; }

        [NotMapped]
        public string Text
        {
            get
            {
                var ret = (Key ? "[key] " + Name : Name);
                return ret;
            }
        }

        public static IEnumerable<Property> GetProperties(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                return null;

            var ret = new List<Property>();

            foreach (var prop in type.GetProperties().Where(it => !it.GetGetMethod().IsVirtual))
            {
                ret.Add(new Property()
                {
                    Name = prop.Name,
                    Key = Attribute.IsDefined(prop, typeof(KeyAttribute)),
                    Required = Attribute.IsDefined(prop, typeof(RequiredAttribute))
                });
            }

            return ret;
        }
    }
}