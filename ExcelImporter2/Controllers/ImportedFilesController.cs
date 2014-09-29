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
    public class ImportedFilesController : ApiController
    {
        private RegistryContext db = new RegistryContext();

        // GET: api/ImportedFiles
        public IQueryable<ImportedFile> GetImportedFiles()
        {
            return db.ImportedFiles;
        }

        // GET: api/ImportedFiles/5
        [ResponseType(typeof(ImportedFile))]
        public async Task<IHttpActionResult> GetImportedFile(string id)
        {
            ImportedFile importedFile = await db.ImportedFiles.FindAsync(id);
            if (importedFile == null)
            {
                return NotFound();
            }

            return Ok(importedFile);
        }

        // PUT: api/ImportedFiles/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutImportedFile(string id, ImportedFile importedFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != importedFile.Id)
            {
                return BadRequest();
            }

            db.Entry(importedFile).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImportedFileExists(id))
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

        // POST: api/ImportedFiles
        [ResponseType(typeof(ImportedFile))]
        public async Task<IHttpActionResult> PostImportedFile(ImportedFile importedFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ImportedFiles.Add(importedFile);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ImportedFileExists(importedFile.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = importedFile.Id }, importedFile);
        }

        // DELETE: api/ImportedFiles/5
        [ResponseType(typeof(ImportedFile))]
        public async Task<IHttpActionResult> DeleteImportedFile(string id)
        {
            ImportedFile importedFile = await db.ImportedFiles.FindAsync(id);
            if (importedFile == null)
            {
                return NotFound();
            }

            db.ImportedFiles.Remove(importedFile);
            await db.SaveChangesAsync();

            return Ok(importedFile);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ImportedFileExists(string id)
        {
            return db.ImportedFiles.Count(e => e.Id == id) > 0;
        }
    }
}