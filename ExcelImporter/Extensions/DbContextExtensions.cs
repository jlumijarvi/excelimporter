using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExcelImporter.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<object> FindObject(this DbContext self, object obj)
        {
            var dbSet = self.Set(obj.GetType());

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
                    foundObj = await (Task<object>)method.Invoke(null, new object[] { self, obj });
                }
            }

            return foundObj;
        }

        public static void VerifyChanges(this DbContext self, object obj)
        {
            var changes = false;
            var entry = self.Entry(obj);
            if (entry.State == EntityState.Modified)
            {
                foreach (var prop in entry.CurrentValues.PropertyNames)
                {
                    if (!object.Equals(entry.CurrentValues[prop], entry.OriginalValues[prop]))
                    {
                        changes = true;
                        break;
                    }
                }

                entry.State = changes ? EntityState.Modified : EntityState.Unchanged;
            }
        }
    }
}