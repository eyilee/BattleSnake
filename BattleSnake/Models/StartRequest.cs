using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Models
{
    public class StartRequest
    {
        public StartRequestGame game { get; set; }
        public int turn { get; set; }
        public StartRequestBoard board { get; set; }
        public StartRequestSnake you { get; set; }
    }

    public class StartRequestGame
    {
        public string id { get; set; }
    }

    public class StartRequestBoard
    {
        public int height { get; set; }
        public int width { get; set; }
        public List<StartRequestCoords> food { get; set; }
        public List<StartRequestSnake> snakes { get; set; }
    }

    public class StartRequestCoords
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class StartRequestSnake
    {
        public string id { get; set; }
        public string name { get; set; }
        public int health { get; set; }
        public List<StartRequestCoords> body { get; set; }
    }
}
