using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BattleSnake.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EndController : ControllerBase
    {
        // POST api/start
        [HttpPost]
        public IActionResult Post([FromBody] string value)
        {
            return Ok();
        }
    }
}