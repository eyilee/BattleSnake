using BattleSnake.Interface;
using BattleSnake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Services
{
    public class AlphaSnake : IGame
    {
        private enum MapType
        {
            Space,
            Food,
            Snake,
            Head,
            WeakHead
        }

        private static readonly string[] Directions = { "up", "down", "left", "right" };

        private int width;
        private int height;

        private MapType[,] walkMap;
        private double[,] scoreMap;

        private const int MapScore = 0;
        private const int BoundScore = -6;
        private const int UnWalkableScore = -100;

        private int SpaceScore = 3;
        private int SpaceScale = 5;

        private int FoodScore;
        private int FoodScale;

        private int SnakeScore = -12;
        private int SnakeScale = 3;

        private int HeadScore = -18;
        private int HeadScale = 1;

        private int WeakHeadScore = 18;
        private int WeakHeadScale = 1;

        private Coords head;
        private int health;
        private int size;

        public void Init(SnakeRequest request)
        {
            width = request.board.width;
            height = request.board.height;

            walkMap = new MapType[width, height];
            scoreMap = new double[width, height];

            UpdateMaps(request);
        }

        private void UpdateMaps(SnakeRequest request)
        {
            ResetMaps();
            SetPlayer(request.you);
            SetSnakes(request.board.snakes);
            SetFoods(request.board.food);
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

        private void SetFoods(List<Coords> foods)
        {
            foreach (var food in foods)
            {
                walkMap[food.x, food.y] = MapType.Food;
            }

            if (health < 30)
            {
                FoodScore = 72;
                FoodScale = 8;
            }
            else if (health < 60)
            {
                FoodScore = 36;
                FoodScale = 4;
            }
            else
            {
                FoodScore = 18;
                FoodScale = 2;
            }
        }

        private void SetSnakes(List<Snake> snakes)
        {
            foreach (var snake in snakes)
            {
                foreach (var body in snake.body)
                {
                    walkMap[body.x, body.y] = MapType.Snake;
                }

                if (size < snake.body.Capacity)
                {
                    walkMap[snake.body[0].x, snake.body[0].y] = MapType.Head;
                }
                else
                {
                    walkMap[snake.body[0].x, snake.body[0].y] = MapType.WeakHead;
                }
            }
        }

        private void SetPlayer(Snake player)
        {
            head = player.body[0];
            health = player.health;
            size = player.body.Capacity;

            foreach (var body in player.body)
            {
                walkMap[body.x, body.y] = MapType.Snake;
            }
        }

        public string GetNextMove(SnakeRequest request)
        {
            UpdateMaps(request);

            CalculateScoreMap();

            return GetDirection();
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
                    ApplyUnWalkableScore(x, y, UnWalkableScore);
                    break;
                case MapType.Head:
                    ApplyScoreMask(x, y, HeadScore, HeadScale);
                    ApplyUnWalkableScore(x, y, UnWalkableScore);
                    break;
                case MapType.WeakHead:
                    ApplyScoreMask(x, y, WeakHeadScore, WeakHeadScale);
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
                        scoreMap[i, j] += score * (1 - distance / (range + 1));
                    }
                }
            }
        }

        private void ApplyUnWalkableScore(int x, int y, int score)
        {
            throw new NotImplementedException();
        }

        private string GetDirection()
        {
            double min = scoreMap.Cast<double>().Min();

            double[] score = { min, min, min, min };

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

            double max = score.Cast<double>().Max();

            int index = Array.FindIndex(score, x => x == max);

            return Directions[index];
        }
    }
}
