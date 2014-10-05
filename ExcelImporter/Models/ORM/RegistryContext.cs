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

        public System.Data.Entity.DbSet<ExcelImporter.Models.HeaderPropertyMapping> HeaderPropertyMappings { get; set; }
    }
}
