using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BattleSnake.Controllers
{
    [Route("api/{snake:alpha}/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        // POST api/ping
        [HttpPost]
        public IActionResult Post()
        {
            return Ok();
        }
    }
}