using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Models
{
    public class MapMove
    {
        public static readonly Coord Identity = new Coord { x = 1, y = 1 };
        public static readonly Coord Up = new Coord { x = 0, y = -1 };
        public static readonly Coord Down = new Coord { x = 0, y = 1 };
        public static readonly Coord Left = new Coord { x = -1, y = 0 };
        public static readonly Coord Right = new Coord { x = 1, y = 0 };
    }
}
