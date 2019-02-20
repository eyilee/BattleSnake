using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleSnake.Models;

namespace BattleSnake.Services
{
    public class GameManager
    {
        private static readonly GameManager instance = new GameManager();

        private readonly Dictionary<string, Game> games = new Dictionary<string, Game>();

        private GameManager() { }

        public static GameManager Instance {
            get {
                return instance;
            }
        }

        public bool CreateGame(StartRequest startRequest)
        {
            string gameId = startRequest.game.id;
            if (games.ContainsKey(gameId))
            {
                return false;
            }

            games.Add(gameId, new Game(startRequest));

            return true;
        }

        public Game GetGame(StartRequest startRequest)
        {
            string gameId = startRequest.game.id;
            if (games.ContainsKey(gameId))
            {
                return games[gameId];
            }

            return null;
        }

        public bool RemoveGame(StartRequest startRequest)
        {
            string gameId = startRequest.game.id;
            if (games.ContainsKey(gameId))
            {
                games.Remove(gameId);
                return true;
            }

            return false;
        }
    }

    public class Game
    {
        private readonly int width;
        private readonly int height;

        private enum MapType
        {
            Space,
            Food,
            Snake,
            Head
        }

        private readonly string[] Directions = { "up", "down", "left", "right" };

        private readonly MapType[,] walkMap;
        private readonly double[,] scoreMap;

        private const double MapScore = 200.0d;
        private const int BoundScore = -9;

        private int SpaceScore = 3;
        private int SpaceScale = 5;

        private int FoodScore = 18;
        private int FoodScale = 3;

        private int SnakeScore = -12;
        private int SnakeScale = 3;

        private int HeadScore = -18;
        private int HeadScale = 1;

        private StartRequestCoords head;
        private int health;

        public Game(StartRequest startRequest)
        {
            width = startRequest.board.width;
            height = startRequest.board.height;

            walkMap = new MapType[width, height];
            scoreMap = new double[width, height];

            ResetMaps();
            PutFoods(startRequest.board.food);
            PutSnakes(startRequest.board.snakes);
            PutPlayer(startRequest.you);
        }

        private void ResetMaps()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    walkMap[i, j] = MapType.Space;
                    scoreMap[i, j] = MapScore;
                }
            }
        }

        private void PutFoods(List<StartRequestCoords> foods)
        {
            foreach (var food in foods)
            {
                walkMap[food.x, food.y] = MapType.Food;
            }
        }

        private void PutSnakes(List<StartRequestSnake> snakes)
        {
            foreach (var snake in snakes)
            {
                foreach (var body in snake.body)
                {
                    walkMap[body.x, body.y] = MapType.Snake;
                }
                walkMap[snake.body[0].x, snake.body[0].y] = MapType.Head;
            }
        }

        private void PutPlayer(StartRequestSnake player)
        {
            head = player.body[0];
            health = player.health;

            foreach (var body in player.body)
            {
                walkMap[body.x, body.y] = MapType.Snake;
            }

            if (health < 60)
            {
                FoodScore = 36;
                FoodScale = 6;
            }
            else if (health < 30)
            {
                FoodScore = 54;
                FoodScale = 8;
            }
        }

        public string NextMove(StartRequest startRequest)
        {
            ResetMaps();
            PutFoods(startRequest.board.food);
            PutSnakes(startRequest.board.snakes);
            PutPlayer(startRequest.you);
            CalculateScoreMap();

            return MoveStep();
        }

        private void CalculateScoreMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CalculateBoundScore(x, y);
                    CalculateWalkableScore(x, y);
                }
            }
        }

        private void CalculateBoundScore(int x, int y)
        {
            if (y == 0 || y == height)
            {
                scoreMap[x, y] += BoundScore;
            }

            if (x == 0 || x == width)
            {
                scoreMap[x, y] += BoundScore;
            }
        }

        private void CalculateWalkableScore(int x, int y)
        {
            switch (walkMap[x, y])
            {
                case MapType.Space:
                    ApplyScoreMask(x, y, SpaceScore, SpaceScale);
                    break;
                case MapType.Food:
                    ApplyScoreMask(x, y, FoodScore, FoodScale);
                    break;
                case MapType.Snake:
                    ApplyScoreMask(x, y, SnakeScore, SnakeScale);
                    scoreMap[x, y] -= 20;
                    break;
                case MapType.Head:
                    ApplyScoreMask(x, y, HeadScore, HeadScale);
                    break;
                default:
                    break;
            }
        }

        private void ApplyScoreMask(int x, int y, int score, int range)
        {
            int up = Math.Max(y - range, 0);
            int down = Math.Min(y + range, height - 1);
            int left = Math.Max(x - range, 0);
            int right = Math.Min(x + range, width - 1);

            for (int i = left; i <= right; i++)
            {
                for (int j = up; j <= down; j++)
                {
                    double distance = Math.Sqrt((i - x) * (i - x) + (j - y) * (j - y));

                    if (distance <= range)
                    {
                        scoreMap[i, j] += score * (1 - distance / range);
                    }
                }
            }
        }

        private string MoveStep()
        {
            double[] score = { 0.0d, 0.0d, 0.0d, 0.0d };

            if (head.y - 1 >= 0) // up
            {
                score[0] = scoreMap[head.x, head.y - 1];
            }

            if (head.y + 1 < height) // down
            {
                score[1] = scoreMap[head.x, head.y + 1];
            }

            if (head.x - 1 >= 0) // left
            {
                score[2] = scoreMap[head.x - 1, head.y];
            }

            if (head.x + 1 < width) // right
            {
                score[3] = scoreMap[head.x + 1, head.y];
            }

            int i = score[0] > score[1] ? 0 : 1;
            int j = score[2] > score[3] ? 2 : 3;
            int k = score[i] > score[j] ? i : j;

            return Directions[k];
        }
    }
}
