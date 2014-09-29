using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ExcelImporter.Models
{
    public class Column
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

        public static IEnumerable<Column> GetColumns(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                return null;

            var ret = new List<Column>();

            foreach (var prop in type.GetProperties())
            {
                ret.Add(new Column()
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