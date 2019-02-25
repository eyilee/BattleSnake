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
        public IActionResult Post(string snake, [FromBody] GameRequest request)
        {
            IGame game = null;
            string color = null;
            string headType = null;
            string tailType = null;
            
            switch (snake)
            {
                case "alpha":
                    game = GameManager.Instance.CreateGame<AlphaSnake>(request.game.id);
                    color = "#66ccff";
                    headType = "bendr";
                    tailType = "pixel";
                    break;
                case "beta":
                    game = GameManager.Instance.CreateGame<BetaSnake>(request.game.id);
                    color = "#ee82ee";
                    headType = "bendr";
                    tailType = "pixel";
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
                color = color,
                headType = headType,
                tailType = tailType
            };

            return Ok(response);
        }
    }
}