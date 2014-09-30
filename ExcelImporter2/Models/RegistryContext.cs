using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExcelImporter.Models
{
    public class RegistryContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public RegistryContext()
            : base("name=RegistryContext")
        {
        }

        public System.Data.Entity.DbSet<ExcelImporter.Models.Person> People { get; set; }

        public System.Data.Entity.DbSet<ExcelImporter.Models.Device> Devices { get; set; }

        public System.Data.Entity.DbSet<ExcelImporter.Models.ImportedTable> ImportedTables { get; set; }

        public System.Data.Entity.DbSet<ExcelImporter.Models.ImportedFile> ImportedFiles { get; set; }

        public System.Data.Entity.DbSet FindSet(Type t)
        {
            var ret = default(System.Data.Entity.DbSet);

            switch (t.ToString())
            {
                case "ExcelImporter.Models.Person":
                    ret = People;
                    break;
                case "ExcelImporter.Models.Device":
                    ret = Devices;
                    break;
                case "ExcelImporter.Models.ImportedTable":
                    ret = ImportedTables;
                    break;
                case "ExcelImporter.Models.ImportedFile":
                    ret = ImportedFiles;
                    break;
                default:
                    break;
            }

            return ret;
        }

        public async Task<object> FindObject(object obj)
        {
            var dbSet = this.Set(obj.GetType());

            var keyValues = new List<object>();
            var keys = obj.GetType().GetProperties().Where(it => Attribute.IsDefined(it, typeof(KeyAttribute))).ToList();
            keys.ForEach(it => keyValues.Add(it.GetValue(obj)));

            var foundObj = await dbSet.FindAsync(keyValues.ToArray());

            if (foundObj == null)
            {
                // Do custom finding based on type
                var method = obj.GetType().GetMethods().SingleOrDefault(it => it.IsStatic && it.Name == "Find");

                if (method != null)
                {
                    foundObj = await (Task<object>) method.Invoke(null, new object[] { this, obj });
                }
            }

            return foundObj;
        }
    }
}
