using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Models
{
    public class GameRequest
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
        public List<Coord> food;
        public List<Snake> snakes;
    }

    public class Coord
    {
        public int x;
        public int y;

        public static Coord operator +(Coord lhs, Coord rhs)
        {
            return new Coord
            {
                x = lhs.x + rhs.x,
                y = lhs.y + rhs.y
            };
        }

        public static bool operator ==(Coord lhs, Coord rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(Coord lhs, Coord rhs)
        {
            return lhs.x != rhs.x || lhs.y != rhs.y;
        }

        public override bool Equals(object obj)
        {
            Coord coord = obj as Coord;
            return coord != null &&
                   x == coord.x &&
                   y == coord.y;
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
        public List<Coord> body;
    }
}
