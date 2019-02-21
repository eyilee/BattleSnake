using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using BattleSnake.Models;
using BattleSnake.Services;
using BattleSnake.Interface;

namespace BattleSnake.Controllers
{
    [Route("api/{snake:alpha}/[controller]")]
    [ApiController]
    public class StartController : ControllerBase
    {
        // POST api/start
        [HttpPost]
        public IActionResult Post(string snake, [FromBody] SnakeRequest request)
        {
            IGame game = null;

            switch (snake)
            {
                case "alpha":
                    game = GameManager.Instance.CreateGame<AlphaSnake>(request.game.id);
                    break;
                case "beta":
                    break;
                case "gamma":
                    break;
                default:
                    break;
            }

            if (game == null)
            {
                return BadRequest();
            }

            game.Init(request);

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