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

        IQueryable<Device> GetDevices();
        Task<Device> GetDevice(string id);
        Task<bool> SaveDevice(string id, Device device);
        Task<Device> AddDevice(Device device);
        Task<Device> DeleteDevice(string id);

        Task<IEnumerable<ImportResult>> ImportFile(string id, string userName, IEnumerable<HeaderPropertyMapping> mappings, bool noSave);
        Task<ImportedFile> DeleteFile(string id, string userName);
    }
}
