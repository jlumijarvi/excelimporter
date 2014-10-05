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
    public class Spreadsheet
    {
        public static IEnumerable<string> SupportedFormats = new string[]
        {
            ".xls",
            ".xlsx",
            ".xlsm",
            ".csv"
        };

        public static ISpreadsheet Create(string fn)
        {
            switch (Path.GetExtension(fn).ToLower())
            {
                case ".xls":
                case ".xlsx":
                case ".xlsm":
                    return new ExcelSpreadsheet(fn);
                case ".csv":
                    return new CsvSpreadsheet(fn);
                default:
                    break;
            }
            return null;
        }

        public static bool IsSupported(string fn)
        {
            var ext = Path.GetExtension(fn).ToLower();
            return SupportedFormats.Contains(ext);
        }
    }
}