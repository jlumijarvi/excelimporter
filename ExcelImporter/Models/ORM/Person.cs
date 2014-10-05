using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExcelImporter.Models
{
    public class Person
    {
        [Key]
        public string ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Comments { get; set; }

        /// <summary>
        /// Searches the given db for obj using "secondary" keys. It is assumed obj is of this type.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static async Task<object> Find(DbContext db, object obj)
        {
            var person = obj as Person;
            var ret = await db.Set<Person>().SingleOrDefaultAsync(it => it.Name == person.Name && it.Email == person.Email);
            return ret;
        }
    }
}
