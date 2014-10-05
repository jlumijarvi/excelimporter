using ExcelImporter.Extensions;
using ExcelImporter.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;


namespace ExcelImporter.Controllers
{
    public class ImportController : ApiController
    {
        private RegistryContext db = new RegistryContext();

        public async Task<IHttpActionResult> Post(string id, [FromBody]IEnumerable<HeaderPropertyMapping> mappings, bool preview = false)
        {
            if (Debugger.IsAttached)
                Thread.Sleep(1000);

            var ret = new List<TableImportResult>();

            var fn = (await db.ImportedFiles.FirstOrDefaultAsync(it => it.Id == id && it.User == Thread.CurrentPrincipal.Identity.Name.ToLower())).Path;

            using (var spreadsheet = Spreadsheet.Create(fn))
            {
                var headerCells = (await spreadsheet.GetHeaderRow()).Where(it => it != null);

                var headers = new Dictionary<int, string>();
                for (int col = 0; col < headerCells.Count(); col++)
                {
                    headers.Add(col, headerCells.ElementAt(col));
                }

                var row = await spreadsheet.GetNextRow();
                for (; row != null; row = await spreadsheet.GetNextRow())
                {
                    var objectsInRow = new List<object>();
                    for (int col = 0; col < row.Count(); col++)
                    {
                        if (!headers.ContainsKey(col))
                            continue;
                        var header = headers[col];
                        var cm = mappings.FirstOrDefault(it => it.Header == header);
                        if (cm == null)
                            continue;

                        var type = Type.GetType(cm.Type);
                        var newObj = objectsInRow.FirstOrDefault(it => it.GetType() == type);
                        if (newObj == null)
                        {
                            newObj = Activator.CreateInstance(type);
                            objectsInRow.Add(newObj);
                        }
                        var prop = newObj.GetType().GetProperty(cm.Property);
                        try
                        {
                            var propType = prop.PropertyType;
                            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                propType = Nullable.GetUnderlyingType(propType);
                            var strValue =  spreadsheet.ConvertCell(col, propType);
                            if (strValue != null)
                            {
                                var val = TypeDescriptor.GetConverter(propType).ConvertFrom(strValue);
                                prop.SetValue(newObj, val);
                            }
                        }
                        catch { }
                    }

                    var changedObjects = new List<object>();

                    foreach (var obj in objectsInRow)
                    {
                        if (!ImportHelper.RepairObject(obj))
                            continue;

                        var foundObj = await db.FindObject(obj);
                        if (foundObj == null)
                        {
                            db.Set(obj.GetType()).Add(obj);
                            changedObjects.Add(obj);
                        }
                        else
                        {
                            var copiedProperties = mappings.Where(it => headers.Values.Contains(it.Header)).Select(it => it.Property);
                            ImportHelper.CopyProperties(obj, foundObj, copiedProperties);
                            changedObjects.Add(foundObj);
                        }
                    }

                    ImportHelper.SetRelations(db, changedObjects);
                    changedObjects.ForEach(it => db.VerifyChanges(it));
                }
            }

            var tables = mappings.Select(it => it.Type).Distinct();
            foreach (var table in tables)
            {
                var type = Type.GetType(table);
                var columns = (from prop in type.GetProperties()
                               where Attribute.IsDefined(prop, typeof(KeyAttribute)) ||
                                Attribute.IsDefined(prop, typeof(ForeignKeyAttribute)) ||
                                mappings.Any(cm => cm.Type == table && cm.Property == prop.Name)
                               select prop.Name).ToList();

                var localObjects = db.Set(type).Local.OfType<object>();

                var addedItems = new List<string[]>();
                foreach (var addedObj in localObjects.Where(it => db.Entry(it).State == EntityState.Added))
                {
                    var entry = db.Entry(addedObj);
                    var data = new List<string>();
                    columns.ForEach(it => data.Add((entry.CurrentValues[it] ?? string.Empty).ToString()));
                    addedItems.Add(data.ToArray());
                }

                var modifiedItems = new List<List<string>>();
                var originalItems = new List<List<string>>();
                foreach (var modifiedObj in localObjects.Where(it => db.Entry(it).State == EntityState.Modified))
                {
                    var entry = db.Entry(modifiedObj);
                    modifiedItems.Add(new List<string>());
                    var data = modifiedItems.Last();
                    originalItems.Add(new List<string>());
                    var origData = originalItems.Last();
                    foreach (var col in columns)
                    {
                        data.Add((entry.CurrentValues[col] ?? string.Empty).ToString());
                        origData.Add((entry.OriginalValues[col] ?? string.Empty).ToString());
                    }
                }

                ret.Add(new TableImportResult()
                {
                    Name = Regex.Replace(table, @".+\.", string.Empty),
                    AddedCount = addedItems.Count,
                    Added = addedItems,
                    ModifiedCount = modifiedItems.Count,
                    Modified = modifiedItems,
                    Original = originalItems,
                    Columns = columns
                });
            }

            if (!preview)
            {
                foreach (var cm in mappings)
                {
                    var foundColumnMapping = await db.HeaderPropertyMapping.Where(it =>
                        string.Compare(cm.Type, it.Type, true) == 0 &&
                        string.Compare(cm.Property, it.Property, true) == 0).FirstOrDefaultAsync();

                    if (foundColumnMapping == null)
                    {
                        foundColumnMapping = db.HeaderPropertyMapping.Add(new HeaderPropertyMapping()
                        {
                            Type = cm.Type,
                            Property = cm.Property
                        });
                    }
                    foundColumnMapping.Header = cm.Header;
                }

                await db.SaveChangesAsync();
            }

            return Ok(ret);
        }

        public async Task<IHttpActionResult> Delete(string id)
        {
            var item = (await db.ImportedFiles.FirstOrDefaultAsync(it => it.Id == id && it.User == Thread.CurrentPrincipal.Identity.Name.ToLower()));
            if (item == null)
                return NotFound();

            try
            {
                if (File.Exists(item.Path))
                    File.Delete(item.Path);
            }
            catch { }

            db.ImportedFiles.Remove(item);
            await db.SaveChangesAsync();

            return Ok(item);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
