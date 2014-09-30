using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExcelImporter.Models
{
    public class ColumnMapping
    {
        public string Header { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
    }
}
