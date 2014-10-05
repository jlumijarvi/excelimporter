using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ExcelImporter.Extensions;
using System.Threading.Tasks;

namespace ExcelImporter.Models
{
    public class ExcelSpreadsheet : ISpreadsheet
    {
        string _fn;
        IWorkbook _workbook { get; set; }
        ISheet _sheet { get; set; }
        int _currentRow = 0;

        public string FileName { get { return _fn; } }

        public ExcelSpreadsheet(string fn)
        {
            _fn = fn;

            var ext = Path.GetExtension(_fn).ToLower();

            using (var fs = new FileStream(_fn, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                switch (ext)
                {
                    case ".xls":
                        _workbook = new HSSFWorkbook(fs);
                        break;
                    case ".xslx":
                    default:
                        _workbook = new XSSFWorkbook(fs);
                        break;
                }
                _sheet = _workbook.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<string>> GetHeaderRow()
        {
            return await GetRow(0);
        }

        public async Task<IEnumerable<string>> GetNextRow()
        {
            return await GetRow(++_currentRow);
        }

        private async Task<IEnumerable<string>> GetRow(int row)
        {
            await Task.Run(() => { });

            if (_workbook != null)
            {
                IRow rowData = _sheet.GetRow(row);
                if (rowData == null)
                    return null;

                var cells = new List<string>();

                for (int col = rowData.FirstCellNum; col <= rowData.LastCellNum; col++)
                {
                    var cell = rowData.GetCell(col, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                    if (cell == null)
                        cells.Add(null);
                    else
                        cells.Add(cell.GetValueAsString());
                }

                return cells;
            }

            return null;
        }

        public string ConvertCell(int col, Type type = null)
        {
            if (_workbook != null)
            {
                IRow rowData = _sheet.GetRow(_currentRow);
                if (rowData == null)
                    return null;
                ICell cell = rowData.GetCell(col + rowData.FirstCellNum, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                return rowData.GetCell(col, MissingCellPolicy.RETURN_NULL_AND_BLANK).GetValueAsString(type);
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}