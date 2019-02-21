﻿using BattleSnake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Interface
{
    public interface IGame
    {
        void Init(SnakeRequest request);
        string NextMove(SnakeRequest request);
    }
}