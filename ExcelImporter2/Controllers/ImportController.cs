using ExcelImporter.Extensions;
using ExcelImporter.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
                for (int col = 0; col < row.PhysicalNumberOfCells; col++)
                {
                    if (!headers.ContainsKey(col))
                        continue;
                    var header = headers[col];
                    var cm = columnMappings.FirstOrDefault(it => it.Header == header);
                    if (cm == null)
                        continue;

                    var type = Type.GetType(cm.Table);
                    var newObj = objectsInRow.FirstOrDefault(it => it.GetType() == type);
                    if (newObj == null)
                    {
                        newObj = Activator.CreateInstance(type);
                        objectsInRow.Add(newObj);
                    }
                    var prop = newObj.GetType().GetProperty(cm.Column);
                    try
                    {
                        var propType = prop.PropertyType;
                        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            propType = Nullable.GetUnderlyingType(propType);
                        var strValue = row.Cells[col].GetValueAsString(propType);
                        var val = TypeDescriptor.GetConverter(propType).ConvertFrom(strValue);
                        prop.SetValue(newObj, val);
                    }
                    catch { }
                }
                foreach (var obj in objectsInRow)
                {
                    ImportHelper.RepairObject(obj);
                }

                var handledObjects = new List<object>();

                foreach (var obj in objectsInRow)
                {
                    var foundObj = await db.FindObject(obj);
                    if (foundObj == null)
                    {
                        db.Set(obj.GetType()).Add(obj);
                        handledObjects.Add(obj);
                    }
                    else
                    {
                        var copiedProperties = columnMappings.Where(it => headers.Values.Contains(it.Header)).Select(it => it.Column);
                        ImportHelper.CopyProperties(obj, foundObj, copiedProperties);
                        handledObjects.Add(foundObj);
                    }
                }

                ImportHelper.SetRelations(db, handledObjects);
            }

            foreach (var table in columnMappings.Select(it => it.Table).Distinct())
            {
                var type = Type.GetType(table);
                var columns = new List<string>();
                foreach (var prop in type.GetProperties())
                {
                    if (Attribute.IsDefined(prop, typeof(KeyAttribute)) ||
                        columnMappings.Any(it => it.Table == table && it.Column == prop.Name))
                    {
                        columns.Add(prop.Name);
                    }
                }

                var changedObjects = db.Set(type).Local.OfType<object>();

                var addedObjects = changedObjects.Where(it => db.Entry(it).State == EntityState.Added).ToList();
                var added = addedObjects.Count;
                var addedItems = new List<string[]>();
                foreach (var addedObj in addedObjects)
                {
                    var entry = db.Entry(addedObj);
                    var data = new List<string>();
                    columns.ForEach(it => data.Add((entry.CurrentValues[it] ?? string.Empty).ToString()));
                    addedItems.Add(data.ToArray());
                }

                var modifiedObjects = changedObjects.Where(it => db.Entry(it).State == EntityState.Modified);
                var modified = 0;
                var modifiedItems = new List<string[]>();
                var originalItems = new List<string[]>();
                foreach (var modifiedObj in modifiedObjects)
                {
                    var entry = db.Entry(modifiedObj);
                    bool differ = false;
                    foreach (var prop in entry.CurrentValues.PropertyNames)
                    {
                        if (!object.Equals(entry.CurrentValues[prop], entry.OriginalValues[prop]))
                        {
                            differ = true;
                            break;
                        }
                    }
                    if (!differ)
                        continue;

                    var data = new List<string>();
                    columns.ForEach(it => data.Add((entry.CurrentValues[it] ?? string.Empty).ToString()));
                    modifiedItems.Add(data.ToArray());
                    var origData = new List<string>();
                    columns.ForEach(it => origData.Add((entry.OriginalValues[it] ?? string.Empty).ToString()));
                    originalItems.Add(origData.ToArray());
                    modified++;
                }

                ret.Add(new TableImportResult()
                {
                    Name = Regex.Replace(table, @".+\.", string.Empty),
                    AddedCount = added,
                    Added = addedItems,
                    ModifiedCount = modified,
                    Modified = modifiedItems,
                    Original = originalItems,
                    Columns = columns
                });
            }

            if (!preview)
            {
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                }
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
