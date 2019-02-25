using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Models
{
    public class SnakeRequest
    {
        public Game game;
        public int turn;
        public Board board;
        public Snake you;
    }

    public class Game
    {
        public string id;
    }

    public class Board
    {
        public int height;
        public int width;
        public List<Coords> food;
        public List<Snake> snakes;
    }

    public class Coords
    {
        public int x;
        public int y;

        public static readonly Coords Identity = new Coords { x = 1, y = 1 };
        public static readonly Coords Up = new Coords { x = 0, y = -1 };
        public static readonly Coords Down = new Coords { x = 0, y = 1 };
        public static readonly Coords Left = new Coords { x = -1, y = 0 };
        public static readonly Coords Right = new Coords { x = 1, y = 0 };

        public static Coords operator +(Coords lhs, Coords rhs)
        {
            return new Coords
            {
                x = lhs.x + rhs.x,
                y = lhs.y + rhs.y
            };
        }

        public static bool operator ==(Coords lhs, Coords rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(Coords lhs, Coords rhs)
        {
            return lhs.x != rhs.x || lhs.y != rhs.y;
        }

        public override bool Equals(object obj)
        {
            Coords coords = obj as Coords;
            return coords != null &&
                   x == coords.x &&
                   y == coords.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }

    public class Snake
    {
        public string id;
        public string name;
        public int health;
        public List<Coords> body;
    }
}
