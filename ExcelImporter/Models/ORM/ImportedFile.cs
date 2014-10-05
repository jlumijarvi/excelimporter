using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ExcelImporter.Models
{
    public class ImportedFile
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string User { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public string OriginalFileName { get; set; }
        [Required]
        public DateTime Created { get; set; }
    }
}