﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExcelImporter.Models
{
    public class TableImportResult
    {
        public string Name { get; set; }
        public IEnumerable<string> Columns { get; set; }
        public int AddedCount { get; set; }
        public IEnumerable<string[]> Added { get; set; }
        public int ModifiedCount { get; set; }
        public IEnumerable<string[]> Modified { get; set; }
    }
}
