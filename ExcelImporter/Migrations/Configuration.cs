namespace ExcelImporter.Migrations
{
    using ExcelImporter.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ExcelImporter.Models.RegistryContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ExcelImporter.Models.RegistryContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            using (var db = new RegistryContext())
            {
                db.People.AddOrUpdate(
                    p => p.ID,
                    new Person { ID = "john@foo.com", Comments = "", Name = "John Foo" },
                    new Person { ID = "john@doe.com", Comments = "", Name = "John Doe" },
                    new Person { ID = "ron@foo.com", Comments = "", Name = "Ron Foo" },
                    new Person { ID = "ron@doe.com", Comments = "", Name = "Ron Doe" }
                    );

                db.ImportedTables.AddOrUpdate(
                    p => p.Name,
                    new ImportedTable { Name = "Device", Type = typeof(Device).FullName },
                    new ImportedTable { Name = "Person", Type = typeof(Person).FullName }
                    );

                db.SaveChanges();
            }
        }
    }
}
