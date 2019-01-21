using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.FeedFlows.DomainModel.Service;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Juna.Feed.Service;
using Juna.Feed.Service.Interfaces;
using Newtonsoft.Json;
using System.Security.Claims;
using Juna.Feed.WebApi.Helpers;
using static Juna.Feed.Repository.RepositoryConstants;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Juna.Feed.Service.Helpers;

namespace Juna.Feed.WebApi.Controllers
{
    [Authorize]
    [Route("Zones")]
    [ApiController]
    public class ZoneController : ControllerBase
    {
        private IZoneService _ZoneService;

        public ZoneController(IZoneService ZoneService)
        {
            _ZoneService = ZoneService;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var ZoneId = Guid.Parse((string)id);

            if (ZoneId == null)
                return BadRequest();

            var Zone = _ZoneService.GetZone(ZoneId);

            if (Zone != null)
            {
                return Ok(Zone);
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteById(string id)
        {
            var ZoneId = Guid.Parse((string)id);

            if (ZoneId == null)
                return BadRequest();

            var Zone = _ZoneService.GetZone(ZoneId);

            if (Zone == null) return NotFound();

            _ZoneService.DeleteZone(ZoneId);

            return Ok();
        }

        [HttpGet("Zones")]
        public ActionResult<List<Zone>> GetZones()
        {
            var Zones = _ZoneService.GetZones();

            if (Zones != null)
            {
                return Ok(Zones);
            }

            return NotFound();
        }

        [HttpPost]
        [Route("SaveAsync")]
        public async Task<IActionResult> SaveZoneAsync([FromBody] Zone Zone)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (Zone == null)
                return BadRequest();

            var ZoneCreated = await _ZoneService.SaveZoneAsync(Zone);

            return CreatedAtAction("GetById", new { id = ZoneCreated.Id }, ZoneCreated);
        }

        [HttpPost]
        [Route("Save")]
        public IActionResult SaveZone([FromBody] Zone Zone)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (Zone == null)
                return BadRequest();

            var ZoneCreated =  _ZoneService.SaveZone(Zone);

            return CreatedAtAction("GetById", new { id = ZoneCreated.Id }, ZoneCreated);
        }
    }
}