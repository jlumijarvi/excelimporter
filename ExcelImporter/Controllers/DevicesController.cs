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
    public class DevicesController : ApiController
    {
        IRegistryRepository _repository;

        public DevicesController(IRegistryRepository repository)
        {
            _repository = repository;
        }

        // GET: api/Devices
        public IQueryable<Device> GetDevices()
        {
            return _repository.GetDevices();
        }

        // GET: api/Devices/5
        [ResponseType(typeof(Device))]
        public async Task<IHttpActionResult> GetDevice(string id)
        {
            Device device = await _repository.GetDevice(id);
            if (device == null)
            {
                return NotFound();
            }

            return Ok(device);
        }

        // PUT: api/Devices/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutDevice(string id, Device device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != device.Imei)
            {
                return BadRequest();
            }

            if (!(await _repository.SaveDevice(id, device)))
                return NotFound();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Devices
        [ResponseType(typeof(Device))]
        public async Task<IHttpActionResult> PostDevice(Device device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            device = await _repository.AddDevice(device);

            if (device == null)
            {
                return Conflict();
            }

            return CreatedAtRoute("DefaultApi", new { id = device.Imei }, device);
        }

        // DELETE: api/Devices/5
        [ResponseType(typeof(Device))]
        public async Task<IHttpActionResult> DeleteDevice(string id)
        {
            Device device = await _repository.DeleteDevice(id);
            if (device == null)
            {
                return NotFound();
            }

            return Ok(device);
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