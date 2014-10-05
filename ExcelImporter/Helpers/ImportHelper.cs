using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExcelImporter.Helpers
{
    public static class ImportHelper
    {
        /// <summary>
        /// Tries to repair object so that it can be imported to db.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool RepairObject(object obj)
        {
            // set missing keys and required attributes
            bool hasValues = false;
            foreach (var prop in obj.GetType().GetProperties())
            {
                var val = prop.GetValue(obj);
                hasValues |= !(val == null || (val is string && ((string)val) == string.Empty));
                if (Attribute.IsDefined(prop, typeof(KeyAttribute)))
                {
                    if (val == null)
                    {
                        if (prop.PropertyType == typeof(Guid))
                            prop.SetValue(obj, Guid.NewGuid());
                        else if (prop.PropertyType == typeof(string))
                            prop.SetValue(obj, Guid.NewGuid().ToString("N").ToString());
                    }
                }
                if (Attribute.IsDefined(prop, typeof(RequiredAttribute)))
                {
                    if (val == null || (val is string && ((string)val) == string.Empty))
                    {
                        val = (prop.PropertyType == typeof(string) ? "-" : Activator.CreateInstance(prop.PropertyType));
                        prop.SetValue(obj, val);
                    }
                }
            }
            return hasValues;
        }

        /// <summary>
        /// Sets the relations (ie. navigation properties) between the given objets if there are any.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="newObjects"></param>
        internal static void SetRelations(DbContext context, IEnumerable<object> newObjects)
        {
            foreach (var obj in newObjects)
            {
                foreach (var prop in obj.GetType().GetProperties().Where(it => it.GetGetMethod().IsVirtual))
                {
                    var type = prop.PropertyType;
                    var reference = newObjects.FirstOrDefault(it => it != obj && it.GetType() == type);
                    var oldReference = prop.GetValue(obj);
                    prop.SetValue(obj, reference);

                    if (((reference == null && oldReference != null)
                        || (oldReference == null && reference != null)
                        || !KeyValuesMatch(reference, oldReference)) && 
                        context.Entry(obj).State == EntityState.Unchanged)
                    {
                        context.Entry(obj).State = EntityState.Modified;
                    }
                }
            }
        }

        internal static object[] GetKeyValues(object obj)
        {
            var ret = new List<object>();
            var keys = obj.GetType().GetProperties().Where(it => Attribute.IsDefined(it, typeof(KeyAttribute))).ToList();

            keys.ForEach(it => ret.Add(it.GetValue(obj)));

            return ret.ToArray();
        }

        internal static void CopyProperties(object source, object target, IEnumerable<string> propNames)
        {
            foreach (var pn in propNames)
            {
                var prop = target.GetType().GetProperty(pn);
                if (prop != null)
                {
                    prop.SetValue(target, prop.GetValue(source));
                }
            }
        }

        internal static bool KeyValuesMatch(object obj1, object obj2)
        {
            foreach (var prop in obj1.GetType().GetProperties().Where(it => Attribute.IsDefined(it, typeof(KeyAttribute))))
            {
                var val1 = prop.GetValue(obj1);
                var val2 = prop.GetValue(obj2);
                if (!object.Equals(val1, val2))
                    return false;
            }
            return true;
        }
    }
}