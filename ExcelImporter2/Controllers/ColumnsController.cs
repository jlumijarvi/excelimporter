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
    public class ColumnsController : ApiController
    {
        [ResponseType(typeof(IEnumerable<Column>))]
        public IHttpActionResult Get(string id)
        {
            var ret = Column.GetColumns(id);
            if (ret == null)
                return NotFound();

            return Ok(ret);
        }
    }
}
