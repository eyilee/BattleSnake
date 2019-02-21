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
    }

    public class Snake
    {
        public string id;
        public string name;
        public int health;
        public List<Coords> body;
    }
}
