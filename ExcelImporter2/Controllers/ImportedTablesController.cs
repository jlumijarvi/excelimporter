using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ExcelImporter.Models;

namespace ExcelImporter.Controllers
{
    public class ImportedTablesController : ApiController
    {
        private RegistryContext db = new RegistryContext();

        // GET: api/ImportedTables
        public IQueryable<ImportedTable> GetImportedTables()
        {
            return db.ImportedTables;
        }

        // GET: api/ImportedTables/5
        [ResponseType(typeof(ImportedTable))]
        public async Task<IHttpActionResult> GetImportedTable(string id)
        {
            ImportedTable importedTable = await db.ImportedTables.FindAsync(id);
            if (importedTable == null)
            {
                return NotFound();
            }

            return Ok(importedTable);
        }

        // PUT: api/ImportedTables/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutImportedTable(string id, ImportedTable importedTable)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != importedTable.Name)
            {
                return BadRequest();
            }

            db.Entry(importedTable).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImportedTableExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ImportedTables
        [ResponseType(typeof(ImportedTable))]
        public async Task<IHttpActionResult> PostImportedTable(ImportedTable importedTable)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ImportedTables.Add(importedTable);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ImportedTableExists(importedTable.Name))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = importedTable.Name }, importedTable);
        }

        // DELETE: api/ImportedTables/5
        [ResponseType(typeof(ImportedTable))]
        public async Task<IHttpActionResult> DeleteImportedTable(string id)
        {
            ImportedTable importedTable = await db.ImportedTables.FindAsync(id);
            if (importedTable == null)
            {
                return NotFound();
            }

            db.ImportedTables.Remove(importedTable);
            await db.SaveChangesAsync();

            return Ok(importedTable);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ImportedTableExists(string id)
        {
            return db.ImportedTables.Count(e => e.Name == id) > 0;
        }
    }
}