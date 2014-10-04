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
    public class ColumnMappingsController : ApiController
    {
        private RegistryContext db = new RegistryContext();

        // GET: api/ColumnMappings
        public IQueryable<ColumnMapping> GetColumnMappings()
        {
            return db.ColumnMappings;
        }

        // GET: api/ColumnMappings/5
        [ResponseType(typeof(ColumnMapping))]
        public async Task<IHttpActionResult> GetColumnMapping(int id)
        {
            ColumnMapping columnMapping = await db.ColumnMappings.FindAsync(id);
            if (columnMapping == null)
            {
                return NotFound();
            }

            return Ok(columnMapping);
        }

        // PUT: api/ColumnMappings/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutColumnMapping(int id, ColumnMapping columnMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != columnMapping.Id)
            {
                return BadRequest();
            }

            db.Entry(columnMapping).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ColumnMappingExists(id))
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

        // POST: api/ColumnMappings
        [ResponseType(typeof(ColumnMapping))]
        public async Task<IHttpActionResult> PostColumnMapping(ColumnMapping columnMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ColumnMappings.Add(columnMapping);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = columnMapping.Id }, columnMapping);
        }

        // DELETE: api/ColumnMappings/5
        [ResponseType(typeof(ColumnMapping))]
        public async Task<IHttpActionResult> DeleteColumnMapping(int id)
        {
            ColumnMapping columnMapping = await db.ColumnMappings.FindAsync(id);
            if (columnMapping == null)
            {
                return NotFound();
            }

            db.ColumnMappings.Remove(columnMapping);
            await db.SaveChangesAsync();

            return Ok(columnMapping);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ColumnMappingExists(int id)
        {
            return db.ColumnMappings.Count(e => e.Id == id) > 0;
        }
    }
}