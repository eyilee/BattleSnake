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
    [Route("api/[controller]")]
    [ApiController]
    public class MoveController : ControllerBase
    {
        // POST api/move
        [HttpPost]
        public IActionResult Post([FromBody] SnakeRequest request)
        {
            IGame game = GameManager.Instance.GetGame(request.game.id);

            if (game == null)
            {
                return BadRequest();
            }

            string move = game.NextMove(request);

            MoveResponse response = new MoveResponse { move = move };

            return Ok(response);
        }
    }
}
