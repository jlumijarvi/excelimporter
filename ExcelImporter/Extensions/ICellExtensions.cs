using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExcelImporter.Extensions
{
    public static class ICellExtensions
    {
        public static string GetValueAsString(this ICell self, Type expectedType = null)
        {
            var ret = default(string);

            switch (self.CellType)
            {
                case CellType.Boolean:
                    ret = self.BooleanCellValue.ToString();
                    break;
                case CellType.Numeric:
                    if (expectedType == typeof(DateTime))
                    {
                        ret = new DateTime(1900, 1, 1).AddDays(self.NumericCellValue - 2).ToString();
                    }
                    else
                    {
                        ret = self.NumericCellValue.ToString();
                    }
                    break;
                case CellType.String:
                    ret = self.StringCellValue;
                    break;
                case CellType.Blank:
                case CellType.Formula:
                case CellType.Error:
                case CellType.Unknown:
                default:
                    ret = string.Empty;
                    break;
            }

            return ret;
        }
    }
}