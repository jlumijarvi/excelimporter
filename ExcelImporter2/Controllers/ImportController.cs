using ExcelImporter.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Web;
using System.ComponentModel;
using ExcelImporter.Extensions;
using System.ComponentModel.DataAnnotations;


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
            using (var fs = new FileStream(fn, FileMode.Open))
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
                var newObjects = new List<object>();
                for (int col = 0; col < row.PhysicalNumberOfCells; col++)
                {
                    if (!headers.ContainsKey(col))
                        continue;
                    var header = headers[col];
                    var cm = columnMappings.FirstOrDefault(it => it.Header == header);
                    if (cm == null)
                        continue;

                    var type = Type.GetType(cm.Table);
                    var newObj = newObjects.FirstOrDefault(it => it.GetType() == type);
                    if (newObj == null)
                    {
                        newObj = Activator.CreateInstance(type);
                        newObjects.Add(newObj);
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
                foreach (var obj in newObjects)
                {
                    db.Set(obj.GetType()).Add(obj);
                }
            }

            foreach (var item in db.ChangeTracker.Entries())
            {
                foreach (var prop in item.Entity.GetType().GetProperties())
                {
                    if (Attribute.IsDefined(prop, typeof(KeyAttribute)))
                    {
                        var val = prop.GetValue(item.Entity);
                        if (val == null)
                        {
                            if (prop.PropertyType == typeof(Guid))
                                prop.SetValue(item.Entity, Guid.NewGuid());
                            else if (prop.PropertyType == typeof(string))
                                prop.SetValue(item.Entity, Guid.NewGuid().ToString("N").ToString());
                        }
                    }
                    if (Attribute.IsDefined(prop, typeof(RequiredAttribute)))
                    {
                        var val = prop.GetValue(item.Entity);
                        if (val == null || (val is string && ((string)val) == string.Empty))
                        {
                            val = (prop.PropertyType == typeof(string) ? "-" : Activator.CreateInstance(prop.PropertyType));
                            prop.SetValue(item.Entity, val);
                            
                        }
                    }
                }
            }

            foreach (var table in columnMappings.Select(it => it.Table).Distinct())
            {
                var type = Type.GetType(table);
                var columns = new List<string>();
                foreach (var prop in type.GetProperties())
                {
                    if (Attribute.IsDefined(prop, typeof(KeyAttribute)) || columnMappings.Any(it => it.Table == table && it.Column == prop.Name))
                    {
                        columns.Add(prop.Name);
                    }
                }
                var changedObjects = db.Set(type).Local.OfType<object>();
                
                var addedObjects = changedObjects.Where(it => db.Entry(it).State == EntityState.Added);
                var added = addedObjects.Count();
                var addedItems = new List<string[]>();
                foreach (var addedObj in addedObjects)
                {
                    var entry = db.Entry(addedObj);
                    var data = new List<string>();
                    columns.ForEach(it => data.Add((entry.CurrentValues[it] ?? string.Empty).ToString()));
                    addedItems.Add(data.ToArray());
                }

                var modifiedObjects = changedObjects.Where(it => db.Entry(it).State == EntityState.Modified);
                var modified = modifiedObjects.Count();
                var modifiedItems = new List<string[]>();
                foreach (var modifiedObj in modifiedObjects)
                {
                    var entry = db.Entry(modifiedObj);
                    var data = new List<string>();
                    columns.ForEach(it => data.Add((entry.CurrentValues[it] ?? string.Empty).ToString()));
                    modifiedItems.Add(data.ToArray());
                }

                ret.Add(new TableImportResult()
                {
                    Name = Regex.Replace(table, @".+\.", string.Empty),
                    AddedCount = added,
                    Added = addedItems,
                    ModifiedCount = modified,
                    Modified = modifiedItems,
                    Columns = columns
                });
            }

            if (!preview)
            {
                try
                {
                    await db.SaveChangesAsync();
                }
                catch { }
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
            catch
            {
            }

            db.ImportedFiles.Remove(item);
            await db.SaveChangesAsync();

            return Ok();
        }

    }
}
