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

namespace ExcelImporter.Models
{
    public class DeviceRepository
    {
        public async Task<IEnumerable<ImportedTable>> GetImportedTables()
        {
            using (var db = new RegistryContext())
            {
                return await db.ImportedTables.ToListAsync();
            }
        }

        public bool IsValidFile(HttpPostedFile postedFile)
        {
            var ext = Path.GetExtension(postedFile.FileName);
            if (ext != ".xls" && ext != ".xlsx")
                return false;

            return true;
        }

        public async Task<string> SaveImportedFile(HttpPostedFile postedFile)
        {
            if (!IsValidFile(postedFile))
                throw new FormatException(postedFile.FileName);

            using (var db = new RegistryContext())
            {
                var uploadPath = HttpContext.Current.Server.MapPath("~/uploads");
                var fileId = Guid.NewGuid().ToString("N").ToString();
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);
                var fn = Path.Combine(uploadPath, fileId);

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

                HSSFWorkbook hssfwb;
                using (var fs = new FileStream(file.Path, FileMode.Open))
                {
                    hssfwb = new HSSFWorkbook(fs);
                }

                ISheet sheet = hssfwb.GetSheetAt(0);
                IRow row = sheet.GetRow(0);

                var tables = await this.GetImportedTables();

                var headers = new List<object>();
                var resolver = new ColumnResolver(tables.Select(it => Type.GetType(it.Type)));
                for (int col = 0; col < row.PhysicalNumberOfCells; col++)
                {
                    var pi = resolver.Resolve(row.Cells[col].StringCellValue);
                    headers.Add(
                        new
                        {
                            ImportColumn = row.Cells[col].StringCellValue,
                            Tables = tables.Select(it => new
                            {
                                Name = it.Name,
                                FullName = it.Type
                            }),
                            SelectedTable = (pi == null ? string.Empty : pi.DeclaringType.FullName),
                            Columns = Column.GetColumns(pi == null ? string.Empty : pi.DeclaringType.FullName),
                            SelectedColumn = (pi == null ? string.Empty : pi.Name)
                        });
                }

                return headers;
            }
        }
    }
}
