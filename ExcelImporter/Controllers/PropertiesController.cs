using ExcelImporter.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace ExcelImporter.Controllers
{
    public class PropertiesController : ApiController
    {
        /// <summary>
        /// Get properties of the given type
        /// </summary>
        /// <param name="id">Type in ORM</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<Property>))]
        public IHttpActionResult Get(string id)
        {
            var ret = Property.GetProperties(id);
            if (ret == null)
                return NotFound();

            return Ok(ret);
        }
    }
}
