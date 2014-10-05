using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using System.Web.Security;
using System.Threading;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using ExcelImporter.Extensions;
using System.Data.Entity.Infrastructure;
using ExcelImporter.Helpers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace ExcelImporter.Models
{
    public class RegistryRepository : IRegistryRepository, IDisposable
    {
        private RegistryContext db = new RegistryContext();

        public async Task<IEnumerable<ImportedTable>> GetImportedTables()
        {
            using (var db = new RegistryContext())
            {
                return await db.ImportedTables.ToListAsync();
            }
        }

        public bool IsValidFile(HttpPostedFile postedFile)
        {
            return Spreadsheet.IsSupported(postedFile.FileName);
        }

        public async Task<string> SaveImportedFile(HttpPostedFile postedFile)
        {
            if (!IsValidFile(postedFile))
                throw new FormatException(postedFile.FileName);

            using (var db = new RegistryContext())
            {
                var ext = Path.GetExtension(postedFile.FileName);
                var uploadPath = HttpContext.Current.Server.MapPath("~/uploads");
                var fileId = Guid.NewGuid().ToString("N").ToString();
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);
                var fn = Path.Combine(uploadPath, fileId + ext);

                using (var file = File.Create(fn))
                {
                    await postedFile.InputStream.CopyToAsync(file);
                }

                // delete previous files for this user
                var user = Thread.CurrentPrincipal.Identity.Name.ToLower();

                foreach (var file in db.ImportedFiles.Where(it => it.User == user))
                {
                    try
                    {
                        File.Delete(file.Path);
                    }
                    catch { }
                    db.ImportedFiles.Remove(file);
                }

                db.ImportedFiles.Add(new ImportedFile()
                {
                    Id = fileId,
                    OriginalFileName = postedFile.FileName,
                    Path = fn,
                    User = Thread.CurrentPrincipal.Identity.Name.ToLower(),
                    Created = DateTime.Now
                });

                await db.SaveChangesAsync();

                return fileId;
            }
        }

        public async Task<IEnumerable<object>> GetHeaderData(string id)
        {
            if (id == null)
                throw new NullReferenceException("id");

            using (var db = new RegistryContext())
            {
                var file = await db.ImportedFiles.FirstOrDefaultAsync(it => it.Id == id);
                if (file == null)
                    return null;

                var headerCells = default(IEnumerable<string>);
                using (var spreadsheet = Spreadsheet.Create(file.Path))
                {
                    headerCells = (await spreadsheet.GetHeaderRow()).Where(it => it != null);
                }

                var tables = await this.GetImportedTables();

                var headers = new List<object>();
                var resolver = new PropertyResolver(tables.Select(it => Type.GetType(it.Type)));

                for (int col = 0; col < headerCells.Count(); col++)
                {
                    var header = headerCells.ElementAt(col);
                    if (string.IsNullOrEmpty(header))
                        continue;

                    var pi = resolver.Resolve(header);
                    headers.Add(
                        new
                        {
                            ImportColumn = header,
                            Tables = tables.Select(it => new
                            {
                                Name = it.Name,
                                FullName = it.Type
                            }),
                            SelectedTable = (pi == null ? string.Empty : pi.DeclaringType.FullName),
                            Columns = Property.GetProperties(pi == null ? string.Empty : pi.DeclaringType.FullName),
                            SelectedColumn = (pi == null ? string.Empty : pi.Name)
                        });
                }
                return headers;
            }
        }

        #region Header property mappings
        public IQueryable<HeaderPropertyMapping> GetHeaderPropertyMappings()
        {
            return db.HeaderPropertyMappings;
        }
        public async Task<HeaderPropertyMapping> GetHeaderPropertyMapping(int id)
        {
            return await db.HeaderPropertyMappings.FindAsync(id);
        }
        public async Task<bool> SaveHeaderPropertyMapping(int id, HeaderPropertyMapping columnMapping)
        {
            db.Entry(columnMapping).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HeaderPropertyMappingExists(id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
            return true;
        }
        public async Task<HeaderPropertyMapping> AddHeaderPropertyMapping(HeaderPropertyMapping columnMapping)
        {
            db.HeaderPropertyMappings.Add(columnMapping);
            await db.SaveChangesAsync();

            return columnMapping;
        }
        public async Task<HeaderPropertyMapping> DeleteHeaderPropertyMapping(int id)
        {
            HeaderPropertyMapping columnMapping = await db.HeaderPropertyMappings.FindAsync(id);
            if (columnMapping == null)
            {
                return null;
            }

            db.HeaderPropertyMappings.Remove(columnMapping);
            await db.SaveChangesAsync();

            return columnMapping;
        }
        private bool HeaderPropertyMappingExists(int id)
        {
            return db.HeaderPropertyMappings.Count(e => e.Id == id) > 0;
        }
        #endregion

        #region Devices
        public IQueryable<Device> GetDevices()
        {
            return db.Devices;
        }
        public async Task<Device> GetDevice(string id)
        {
            Device device = await db.Devices.FindAsync(id);
            if (device == null)
            {
                return null;
            }

            return device;
        }
        public async Task<bool> SaveDevice(string id, Device device)
        {
            db.Entry(device).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }
        public async Task<Device> AddDevice(Device device)
        {
            db.Devices.Add(device);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DeviceExists(device.Imei))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return device;
        }
        public async Task<Device> DeleteDevice(string id)
        {
            Device device = await db.Devices.FindAsync(id);
            if (device == null)
            {
                return null;
            }

            db.Devices.Remove(device);
            await db.SaveChangesAsync();

            return device;
        }
        private bool DeviceExists(string id)
        {
            return db.Devices.Count(e => e.Imei == id) > 0;
        }
        #endregion


        public async Task<IEnumerable<ImportResult>> ImportFile(string fileId, string userName, IEnumerable<HeaderPropertyMapping> mappings, bool noSave)
        {
            var fn = (await db.ImportedFiles.FirstOrDefaultAsync(it => it.Id == fileId && it.User == userName)).Path;

            if (fn == null)
                return null;

            var ret = new List<ImportResult>();

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
                            var strValue = spreadsheet.ConvertCell(col, propType);
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

                ret.Add(new ImportResult()
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

            if (!noSave)
            {
                foreach (var cm in mappings)
                {
                    var foundColumnMapping = await db.HeaderPropertyMappings.Where(it =>
                        string.Compare(cm.Type, it.Type, true) == 0 &&
                        string.Compare(cm.Property, it.Property, true) == 0).FirstOrDefaultAsync();

                    if (foundColumnMapping == null)
                    {
                        foundColumnMapping = db.HeaderPropertyMappings.Add(new HeaderPropertyMapping()
                        {
                            Type = cm.Type,
                            Property = cm.Property
                        });
                    }
                    foundColumnMapping.Header = cm.Header;
                }

                await db.SaveChangesAsync();
            }

            return ret;
        }

        public async Task<ImportedFile> DeleteFile(string id, string userName)
        {
            var item = (await db.ImportedFiles.FirstOrDefaultAsync(it => it.Id == id && it.User == userName));
            if (item == null)
                return null;

            try
            {
                if (File.Exists(item.Path))
                    File.Delete(item.Path);
            }
            catch { }

            db.ImportedFiles.Remove(item);
            await db.SaveChangesAsync();

            return item;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
        }
    }
}
