using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExcelImporter.Models
{
    public interface IRegistryRepository: IDisposable
    {
        Task<IEnumerable<ImportedTable>> GetImportedTables();
        bool IsValidFile(HttpPostedFile postedFile);
        Task<string> SaveImportedFile(HttpPostedFile postedFile);
        Task<IEnumerable<object>> GetHeaderData(string id);

        IQueryable<HeaderPropertyMapping> GetHeaderPropertyMappings();
        Task<HeaderPropertyMapping> GetHeaderPropertyMapping(int id);
        Task<bool> SaveHeaderPropertyMapping(int id, HeaderPropertyMapping columnMapping);
        Task<HeaderPropertyMapping> AddHeaderPropertyMapping(HeaderPropertyMapping columnMapping);
        Task<HeaderPropertyMapping> DeleteHeaderPropertyMapping(int id);
    }
}