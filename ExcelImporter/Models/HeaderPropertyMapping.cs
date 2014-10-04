using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ExcelImporter.Models
{
    public class HeaderPropertyMapping
    {
        [Key]
        public int Id { get; set; }
        public string Header { get; set; }
        public string Type { get; set; }
        public string Property { get; set; }
    }
}
