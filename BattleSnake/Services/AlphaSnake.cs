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
            Head,
            WeakHead,
            Body
        }

        private static readonly string[] Directions = { "up", "down", "left", "right" };

        private const double Sqrt2 = 1.415;
        private const double MapScore = 0;
        private const double UnWalkableScore = -100;

        private int width;
        private int height;
        private int mapSize;

        private MapType[,] walkMap;
        private double[,] scoreMap;

        private const double BoundScore = -3;
        private const int BoundScale = 1;

        private const double SpaceScore = 3;
        private const int SpaceScale = 3;

        private double FoodScore;
        private int FoodScale;

        private const double HeadScore = -48;
        private const int HeadScale = 1;

        private const double WeakHeadScore = 24;
        private const int WeakHeadScale = 1;

        private const double BodyScore = -3;
        private const int BodyScale = 1;

        private Coords head;
        private int health;
        private int bodySize;

        public void Init(SnakeRequest request)
        {
            width = request.board.width;
            height = request.board.height;
            mapSize = (width + height) / 2;

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

        private void SetPlayer(Snake player)
        {
            head = player.body[0];
            health = player.health;
            bodySize = player.body.Capacity;

            foreach (var body in player.body)
            {
                walkMap[body.x, body.y] = MapType.Body;
            }

            if (health < 30)
            {
                FoodScore = 36;
                FoodScale = 4;
            }
            else if (health < 60)
            {
                FoodScore = 12;
                FoodScale = 2;
            }
            else
            {
                FoodScore = SpaceScore;
                FoodScale = SpaceScale;
            }

            if (bodySize < mapSize / 2)
            {
                FoodScore = FoodScore * 1.5;
            }
            else if (bodySize < mapSize)
            {
                FoodScore = FoodScore * 1.2;
            }
        }

        private void SetSnakes(List<Snake> snakes)
        {
            foreach (var snake in snakes)
            {
                foreach (var body in snake.body)
                {
                    walkMap[body.x, body.y] = MapType.Body;
                }

                if (bodySize <= snake.body.Capacity)
                {
                    walkMap[snake.body[0].x, snake.body[0].y] = MapType.Head;
                }
                else
                {
                    walkMap[snake.body[0].x, snake.body[0].y] = MapType.WeakHead;
                }
            }
        }

        private void SetFoods(List<Coords> foods)
        {
            foreach (var food in foods)
            {
                walkMap[food.x, food.y] = MapType.Food;
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
                ApplyScoreMask(x, y, BoundScore, BoundScale);
            }

            if (x == 0 || x == width)
            {
                ApplyScoreMask(x, y, BoundScore, BoundScale);
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
                case MapType.Head:
                    ApplyScoreMask(x, y, HeadScore, HeadScale);
                    ApplyUnWalkableScore(x, y);
                    break;
                case MapType.WeakHead:
                    ApplyScoreMask(x, y, WeakHeadScore, WeakHeadScale);
                    ApplyUnWalkableScore(x, y);
                    break;
                case MapType.Body:
                    ApplyScoreMask(x, y, BodyScore, BodyScale);
                    ApplyUnWalkableScore(x, y);
                    break;
                default:
                    break;
            }
        }

        private void ApplyScoreMask(int x, int y, double score, int range)
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

                    if (distance <= range * Sqrt2)
                    {
                        scoreMap[i, j] += score * (1 - distance / (range * Sqrt2));
                    }
                }
            }
        }

        private void ApplyUnWalkableScore(int x, int y)
        {
            scoreMap[x, y] += UnWalkableScore;
        }

        private string GetDirection()
        {
            double min = scoreMap.Cast<double>().Min();

            double[] score = { min, min, min, min };

            int up = GetLastSpace(head.x, head.y - 1);
            int down = GetLastSpace(head.x, head.y + 1);
            int left = GetLastSpace(head.x - 1, head.y);
            int right = GetLastSpace(head.x + 1, head.y);

            if (up > bodySize / 2)
            {
                score[0] = scoreMap[head.x, head.y - 1];
            }

            if (down > bodySize / 2)
            {
                score[1] = scoreMap[head.x, head.y + 1];
            }

            if (left > bodySize / 2)
            {
                score[2] = scoreMap[head.x - 1, head.y];
            }

            if (right > bodySize / 2)
            {
                score[3] = scoreMap[head.x + 1, head.y];
            }

            double max = score.Cast<double>().Max();

            int index = Array.FindIndex(score, x => x == max);

            return Directions[index];
        }

        private int GetLastSpace(int x, int y, int length = 0)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return length;
            }

            switch (walkMap[x, y])
            {
                case MapType.Space:
                case MapType.Food:
                    break;
                case MapType.Head:
                case MapType.WeakHead:
                case MapType.Body:
                    return length;
                default:
                    break;
            }

            if (length > bodySize / 2)
            {
                return length;
            }

            MapType prev = walkMap[x, y];
            walkMap[x, y] = MapType.Body;

            int[] spaces = { 0, 0, 0, 0 };
            spaces[0] = GetLastSpace(x, y + 1, length + 1);
            spaces[1] = GetLastSpace(x, y - 1, length + 1);
            spaces[2] = GetLastSpace(x - 1, y, length + 1);
            spaces[3] = GetLastSpace(x + 1, y, length + 1);

            walkMap[x, y] = prev;

            return spaces.Cast<int>().Max();
        }
    }
}
