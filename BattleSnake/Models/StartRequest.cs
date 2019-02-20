using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Models
{
    public class StartRequest
    {
        public StartRequestGame game;
        public int turn;
        public StartRequestBoard board;
        public StartRequestSnake you;
    }

    public class StartRequestGame
    {
        public string id;
    }

    public class StartRequestBoard
    {
        public int height;
        public int width;
        public List<StartRequestCoords> food;
        public List<StartRequestSnake> snakes;
    }

    public class StartRequestCoords
    {
        public int x;
        public int y;
    }

    public class StartRequestSnake
    {
        public string id;
        public string name;
        public int health;
        public List<StartRequestCoords> body;
    }
}
