using ExcelImporter.Extensions;
using ExcelImporter.Helpers;
using ExcelImporter.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;


namespace ExcelImporter.Controllers
{
    public class ImportController : ApiController
    {
        IRegistryRepository _repository;

        public ImportController(IRegistryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IHttpActionResult> Post(string id, [FromBody]IEnumerable<HeaderPropertyMapping> mappings, bool preview = false)
        {
            if (Debugger.IsAttached)
                Thread.Sleep(1000);

            var ret = await _repository.ImportFile(id, Thread.CurrentPrincipal.Identity.Name.ToLower(), mappings, preview);

            if (ret == null)
                return NotFound();

            return Ok(ret);
        }

        public async Task<IHttpActionResult> Delete(string id)
        {
            var item = (await _repository.DeleteFile(id, Thread.CurrentPrincipal.Identity.Name.ToLower()));
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
