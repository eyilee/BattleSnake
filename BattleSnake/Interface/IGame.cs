using BattleSnake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Interface
{
    public interface IGame
    {
        string NextMove { get; set; }

        void Init(GameRequest request);
        void Update(GameRequest request);
    }
}
