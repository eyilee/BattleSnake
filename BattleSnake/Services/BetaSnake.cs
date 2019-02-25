using BattleSnake.Interface;
using BattleSnake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Services
{
    public class BetaSnake : IGame
    {
        private enum MapType
        {
            Space = 0,
            Wall,
            Head,
            WeakHead,
            Body,
            Tail,
            Food,
            RaceFood
        }

        private static readonly string[] Directions = { "up", "down", "left", "right" };

        private const double Sqrt2 = 1.415;
        private const double UnWalkableScore = -200;

        private int width;
        private int height;
        private int mapSize;

        private MapType[,] walkMap;
        private double[,] scoreMap;

        private const double SpaceScore = 3;
        private const int SpaceScale = 1;

        private double FoodScore;
        private int FoodScale;

        private const double RaceFoodScore = -48;
        private const int RaceFoodScale = 1;

        private const double HeadScore = -144;
        private const int HeadScale = 1;

        private const double WeakHeadScore = 48;
        private const int WeakHeadScale = 1;

        private const double BodyScore = -3;
        private const int BodyScale = 1;

        private const double TailScore = 3;
        private const int TailScale = 1;

        private Snake player;
        private Coords head;
        private int health;
        private int bodySize;

        private List<Snake> rivals;
        private List<Coords> foods;

        public void Init(SnakeRequest request)
        {
            width = request.board.width + 2;
            height = request.board.height + 2;
            mapSize = (width + height) / 2;

            walkMap = new MapType[width, height];
            scoreMap = new double[width, height];

            SetData(request);

            UpdateMaps();
        }

        private void SetData(SnakeRequest request)
        {
            SetPlayer(request.you);
            SetRivals(request.board.snakes);
            SetFoods(request.board.food);
        }

        private void SetPlayer(Snake you)
        {
            player = you;

            ShiftCoords(player.body);
        }

        private void SetRivals(List<Snake> snakes)
        {
            rivals = snakes.Where(x => x.id != player.id).ToList();

            foreach (Snake rival in rivals)
            {
                ShiftCoords(rival.body);
            }
        }

        private void SetFoods(List<Coords> food)
        {
            foods = food;

            ShiftCoords(foods);
        }

        private void ShiftCoords(List<Coords> coords)
        {
            foreach (Coords coord in coords)
            {
                coord.x += 1;
                coord.y += 1;
            }
        }

        private void UpdateMaps()
        {
            ResetMaps();
            UpdateWalls();
            UpdatePlayer();
            UpdateRivals();
            UpdateFood();
        }

        private void ResetMaps()
        {
            Array.Clear(walkMap, 0, walkMap.Length);
            Array.Clear(scoreMap, 0, scoreMap.Length);
        }

        private void UpdateWalls()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        walkMap[x, y] = MapType.Wall;
                    }
                }
            }
        }

        private void UpdatePlayer()
        {
            head = player.body[0];
            health = player.health;
            bodySize = player.body.Count;

            foreach (Coords body in player.body)
            {
                walkMap[body.x, body.y] = MapType.Body;
            }

            if (player.body.Count > 2)
            {
                Coords[] tails = player.body.TakeLast(2).ToArray();

                if (tails[0].x != tails[1].x || tails[0].y != tails[1].y)
                {
                    walkMap[tails[1].x, tails[1].y] = MapType.Tail;
                }
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
                FoodScore = FoodScore * 4;
            }
            else if (bodySize < mapSize)
            {
                FoodScore = FoodScore * 2;
            }
        }

        private void UpdateRivals()
        {
            foreach (Snake rival in rivals)
            {
                foreach (Coords body in rival.body)
                {
                    walkMap[body.x, body.y] = MapType.Body;
                }

                if (rival.body.Count >= player.body.Count)
                {
                    walkMap[rival.body[0].x, rival.body[0].y] = MapType.Head;
                }
                else
                {
                    walkMap[rival.body[0].x, rival.body[0].y] = MapType.WeakHead;
                }

                if (rival.body.Count >= 2)
                {
                    Coords[] tails = rival.body.TakeLast(2).ToArray();

                    if (tails[0].x != tails[1].x || tails[0].y != tails[1].y)
                    {
                        walkMap[tails[1].x, tails[1].y] = MapType.Tail;
                    }
                }
            }
        }

        private void UpdateFood()
        {
            foreach (Coords food in foods)
            {
                if (IsRaceFood(food.x, food.y))
                {
                    walkMap[food.x, food.y] = MapType.RaceFood;
                }
                else
                {
                    walkMap[food.x, food.y] = MapType.Food;
                }
            }
        }

        private bool IsRaceFood(int x, int y)
        {
            if (walkMap[x, y - 1] == MapType.Head ||
                walkMap[x, y + 1] == MapType.Head ||
                walkMap[x - 1, y] == MapType.Head ||
                walkMap[x + 1, y] == MapType.Head
                )
            {
                return true;
            }

            return false;
        }

        public string GetNextMove(SnakeRequest request)
        {
            SetData(request);

            UpdateMaps();

            CalculateScoreMap();

            return GetDirection();
        }

        private void CalculateScoreMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CalculateWalkableScore(x, y);
                }
            }
        }

        private void CalculateWalkableScore(int x, int y)
        {
            switch (walkMap[x, y])
            {
                case MapType.Space:
                    ApplyScoreMask(x, y, SpaceScore, SpaceScale);
                    break;
                case MapType.Wall:
                    ApplyUnWalkableScore(x, y);
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
                    ApplyUnWalkableScore(x, y);
                    break;
                case MapType.Tail:
                    ApplyScoreMask(x, y, TailScore, TailScale);
                    break;
                case MapType.Food:
                    ApplyScoreMask(x, y, FoodScore, FoodScale);
                    break;
                case MapType.RaceFood:
                    ApplyScoreMask(x, y, RaceFoodScore, RaceFoodScale);
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

            // up
            {
                int space = GetLastSpace(head.x, head.y - 1);

                if (space <= bodySize / 2)
                {
                    scoreMap[head.x, head.y - 1] += UnWalkableScore * (1 - space / (bodySize / 2));
                }

                score[0] = scoreMap[head.x, head.y - 1];
            }

            // down
            {
                int space = GetLastSpace(head.x, head.y + 1);

                if (space <= bodySize / 2)
                {
                    scoreMap[head.x, head.y + 1] += UnWalkableScore * (1 - space / (bodySize / 2));
                }

                score[1] = scoreMap[head.x, head.y + 1];
            }

            // left
            {
                int space = GetLastSpace(head.x - 1, head.y);

                if (space <= bodySize / 2)
                {
                    scoreMap[head.x - 1, head.y] += UnWalkableScore * (1 - space / (bodySize / 2));
                }

                score[2] = scoreMap[head.x - 1, head.y];
            }

            // right
            {
                int space = GetLastSpace(head.x + 1, head.y);

                if (space <= bodySize / 2)
                {
                    scoreMap[head.x + 1, head.y] += UnWalkableScore * (1 - space / (bodySize / 2));
                }

                score[3] = scoreMap[head.x + 1, head.y];
            }

            double max = score.Cast<double>().Max();

            int index = Array.FindIndex(score, x => x == max);

            return Directions[index];
        }

        private int GetLastSpace(int x, int y, int length = 0)
        {
            switch (walkMap[x, y])
            {
                case MapType.Space:
                    break;
                case MapType.Wall:
                case MapType.Head:
                case MapType.WeakHead:
                case MapType.Body:
                    return length;
                case MapType.Tail:
                case MapType.Food:
                case MapType.RaceFood:
                    break;
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
