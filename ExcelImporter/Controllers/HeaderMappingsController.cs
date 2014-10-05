using ExcelImporter.Models;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace ExcelImporter.Controllers
{
    public class HeaderMappingsController : ApiController
    {
        IRegistryRepository _repository;

        public HeaderMappingsController(IRegistryRepository repository)
        {
            _repository = repository;
        }

        // GET: api/ColumnMappings
        public IQueryable<HeaderPropertyMapping> GetHeaderMappings()
        {
            return _repository.GetHeaderPropertyMappings();
        }

        // GET: api/ColumnMappings/5
        [ResponseType(typeof(HeaderPropertyMapping))]
        public async Task<IHttpActionResult> GetHeaderMapping(int id)
        {
            HeaderPropertyMapping headerMapping = await _repository.GetHeaderPropertyMapping(id);
            if (headerMapping == null)
            {
                return NotFound();
            }

            return Ok(headerMapping);
        }

        // PUT: api/ColumnMappings/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutHeaderMapping(int id, HeaderPropertyMapping headerMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != headerMapping.Id)
            {
                return BadRequest();
            }

            if (!(await _repository.SaveHeaderPropertyMapping(id, headerMapping)))
            {
                return NotFound();
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ColumnMappings
        [ResponseType(typeof(HeaderPropertyMapping))]
        public async Task<IHttpActionResult> PostHeaderMapping(HeaderPropertyMapping headerMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _repository.AddHeaderPropertyMapping(headerMapping);

            return CreatedAtRoute("DefaultApi", new { id = headerMapping.Id }, headerMapping);
        }

        // DELETE: api/ColumnMappings/5
        [ResponseType(typeof(HeaderPropertyMapping))]
        public async Task<IHttpActionResult> DeleteHeaderMapping(int id)
        {
            var headerMapping = await _repository.DeleteHeaderPropertyMapping(id);

            if (headerMapping == null)
            {
                return NotFound();
            }

            return Ok(headerMapping);
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