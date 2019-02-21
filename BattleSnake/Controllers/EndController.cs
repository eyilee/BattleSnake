using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleSnake.Models;
using BattleSnake.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BattleSnake.Controllers
{
    [Route("api/{snake:alpha}/[controller]")]
    [ApiController]
    public class EndController : ControllerBase
    {
        // POST api/end
        [HttpPost]
        public IActionResult Post([FromBody] SnakeRequest request)
        {
            GameManager.Instance.RemoveGame(request.game.id);

            return Ok();
        }
    }
}