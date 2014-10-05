using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExcelImporter.Models
{
    public interface ISpreadsheet : IDisposable
    {
        string FileName { get; }
        Task<IEnumerable<string>> GetHeaderRow();
        Task<IEnumerable<string>> GetNextRow();
        string ConvertCell(int col, Type type = null);
    }
}
