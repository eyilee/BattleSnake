using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using BattleSnake.Models;
using BattleSnake.Services;

namespace BattleSnake.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StartController : ControllerBase
    {
        // POST api/start
        [HttpPost]
        public IActionResult Post([FromBody] StartRequest startRequest)
        {
            if (GameManager.Instance.CreateGame(startRequest) == false)
            {
                return BadRequest();
            }

            StartResponse response = new StartResponse
            {
                color = "#66ccff",
                headType = "bendr",
                tailType = "pixel"
            };

            return Ok(response);
        }
    }
}