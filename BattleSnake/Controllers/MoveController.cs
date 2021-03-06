﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleSnake.Interface;
using BattleSnake.Models;
using BattleSnake.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BattleSnake.Controllers
{
    [Route("api/{snake:alpha}/[controller]")]
    [ApiController]
    public class MoveController : ControllerBase
    {
        // POST api/move
        [HttpPost]
        public IActionResult Post([FromBody] GameRequest request)
        {
            IGame game = GameManager.Instance.GetGame(request.game.id);

            if (game == null)
            {
                return BadRequest();
            }

            game.Update(request);

            MoveResponse response = new MoveResponse { move = game.NextMove };

            return Ok(response);
        }
    }
}
