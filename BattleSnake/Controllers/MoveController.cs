using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BattleSnake.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoveController : ControllerBase
    {
        // POST api/move
        [HttpPost]
        public IActionResult Post([FromBody] string value)
        {
            string[] directions = { "up", "down", "left", "right" };
            return new OkObjectResult(new { });
        }
    }
}
