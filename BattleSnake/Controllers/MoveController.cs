using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleSnake.Models;
using BattleSnake.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BattleSnake.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoveController : ControllerBase
    {
        // POST api/move
        [HttpPost]
        public IActionResult Post([FromBody] StartRequest startRequest)
        {
            Game game = GameManager.Instance.GetGame(startRequest);
            if (game == null)
            {
                return BadRequest();
            }

            string move = game.NextMove(startRequest);

            return Ok(new MoveResponse { move = move });
        }
    }
}
