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

        public async Task<IHttpActionResult> Post(string id, [FromBody]ColumnMapping[] columnMappings, bool preview = false)
        {
            var ret = new List<TableImportResult>();

            HSSFWorkbook hssfwb;
            var fn = (await db.ImportedFiles.FirstOrDefaultAsync(it => it.Id == id && it.User == Thread.CurrentPrincipal.Identity.Name.ToLower())).Path;
            using (var fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                hssfwb = new HSSFWorkbook(fs);
            }

            ISheet sheet = hssfwb.GetSheetAt(0);
            IRow headerRow = sheet.GetRow(0);

            var headers = new Dictionary<int, string>();
            for (int col = 0; col < headerRow.PhysicalNumberOfCells; col++)
            {
                headers.Add(col, headerRow.Cells[col].StringCellValue);
            }

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                var objectsInRow = new List<object>();
                for (int col = 0; col <= row.LastCellNum; col++)
                {
                    if (!headers.ContainsKey(col))
                        continue;
                    var header = headers[col];
                    var cm = columnMappings.FirstOrDefault(it => it.Header == header);
                    if (cm == null)
                        continue;

                    var type = Type.GetType(cm.Type);
                    var newObj = objectsInRow.FirstOrDefault(it => it.GetType() == type);
                    if (newObj == null)
                    {
                        newObj = Activator.CreateInstance(type);
                        objectsInRow.Add(newObj);
                    }
                    var prop = newObj.GetType().GetProperty(cm.Field);
                    try
                    {
                        var propType = prop.PropertyType;
                        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            propType = Nullable.GetUnderlyingType(propType);
                        var strValue = row.GetCell(col, MissingCellPolicy.RETURN_NULL_AND_BLANK).GetValueAsString(propType);
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
                        var copiedProperties = columnMappings.Where(it => headers.Values.Contains(it.Header)).Select(it => it.Field);
                        ImportHelper.CopyProperties(obj, foundObj, copiedProperties);
                        changedObjects.Add(foundObj);
                    }
                }

                ImportHelper.SetRelations(db, changedObjects);
                changedObjects.ForEach(it => db.VerifyChanges(it));
            }

            var tables = columnMappings.Select(it => it.Type).Distinct();
            foreach (var table in tables)
            {
                var type = Type.GetType(table);
                var columns = (from prop in type.GetProperties()
                               where Attribute.IsDefined(prop, typeof(KeyAttribute)) ||
                                Attribute.IsDefined(prop, typeof(ForeignKeyAttribute)) ||
                                columnMappings.Any(cm => cm.Type == table && cm.Field == prop.Name)
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
                foreach (var cm in columnMappings)
                {
                    var foundColumnMapping = await db.ColumnMappings.Where(it =>
                        string.Compare(cm.Type, it.Type, true) == 0 &&
                        string.Compare(cm.Field, it.Field, true) == 0).FirstOrDefaultAsync();

                    if (foundColumnMapping == null)
                    {
                        foundColumnMapping = db.ColumnMappings.Add(new ColumnMapping()
                        {
                            Type = cm.Type,
                            Field = cm.Field
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
