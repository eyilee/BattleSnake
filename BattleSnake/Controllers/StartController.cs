using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using BattleSnake.Models;

namespace BattleSnake.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StartController : ControllerBase
    {
        // POST api/start
        [HttpPost]
        public IActionResult Post([FromBody] string value)
        {
            StartRequest startRequest = JsonConvert.DeserializeObject<StartRequest>(value);
            //return new OkObjectResult();
            return Ok();
        }
    }
}