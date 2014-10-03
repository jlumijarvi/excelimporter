using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ExcelImporter.Models
{
    public class Device
    {
        [Key]
        public string Imei { get; set; }
        [Required]
        public string Brand { get; set; }
        public string Type { get; set; }
        [Required]
        public string Model { get; set; }
        [ForeignKey("Owner")]
        public string Owner_ID { get; set; }
        public virtual Person Owner { get; set; }
    }
}
