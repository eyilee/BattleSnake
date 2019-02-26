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
            MyBody,
            Tail,
            Food,
            RaceFood
        }

        private static readonly Coord[] Moves = {
            MapMove.Up,
            MapMove.Down,
            MapMove.Left,
            MapMove.Right
        };

        private static readonly string[] Directions = {
            "up",
            "down",
            "left",
            "right"
        };

        private const double Sqrt2 = 1.415;
        private const double UnWalkableScore = -200;

        private int width;
        private int height;
        private int mapSize;

        private Map<MapType> walkMap;
        private Map<double> scoreMap;

        private const double SpaceScore = 5;
        private const int SpaceScale = 2;

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

        private const double MyBodyScore = -1;
        private const int MyBodyScale = 1;

        private const double TailScore = 5;
        private const int TailScale = 2;

        private Snake player;
        private int bodySize;

        private List<Snake> snakes;
        private List<Coord> food;

        public void Init(GameRequest request)
        {
            width = request.board.width + 2;
            height = request.board.height + 2;
            mapSize = width + height;

            walkMap = new Map<MapType>(width, height);
            scoreMap = new Map<double>(width, height);

            Update(request);
        }

        private void Update(GameRequest request)
        {
            SetPlayer(request.you);
            SetSnakes(request.board.snakes);
            SetFoods(request.board.food);

            ResetMaps();
            UpdateWalls();
            UpdatePlayer();
            UpdateSnakes();
            UpdateFoods();
        }

        private void SetPlayer(Snake player)
        {
            for (int i = 0; i < player.body.Count; i++)
            {
                player.body[i] += MapMove.Identity;
            }

            this.player = player;
            bodySize = this.player.body.Count;

            if (player.health < 30)
            {
                FoodScore = 48;
                FoodScale = 4;
            }
            else if (player.health < 60)
            {
                FoodScore = 24;
                FoodScale = 3;
            }
            else
            {
                FoodScore = 12;
                FoodScale = 2;
            }

            if (bodySize < mapSize / 2)
            {
                FoodScore *= 3;
            }
            else if (bodySize < mapSize)
            {
                FoodScore *= 2;
            }
        }

        private void SetSnakes(List<Snake> snakes)
        {
            for (int i = 0; i < snakes.Count; i++)
            {
                for (int j = 0; j < snakes[i].body.Count; j++)
                {
                    snakes[i].body[j] += MapMove.Identity;
                }
            }

            this.snakes = snakes.Where(x => x.id != player.id).ToList();
        }

        private void SetFoods(List<Coord> food)
        {
            for (int i = 0; i < food.Count; i++)
            {
                food[i] += MapMove.Identity;
            }

            this.food = food;
        }

        private void ResetMaps()
        {
            walkMap.Clear();
            scoreMap.Clear();
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
            foreach (Coord body in player.body)
            {
                walkMap[body] = MapType.MyBody;
            }

            if (bodySize > 2)
            {
                Coord[] tails = player.body.TakeLast(2).ToArray();

                if (tails[0] != tails[1])
                {
                    walkMap[tails[1]] = MapType.Tail;
                }
            }
        }

        private void UpdateSnakes()
        {
            foreach (Snake snake in snakes)
            {
                foreach (Coord body in snake.body)
                {
                    walkMap[body] = MapType.Body;
                }

                if (snake.body.Count >= bodySize)
                {
                    walkMap[snake.body[0]] = MapType.Head;
                }
                else
                {
                    walkMap[snake.body[0]] = MapType.WeakHead;
                }

                if (snake.body.Count >= 2)
                {
                    Coord[] tails = snake.body.TakeLast(2).ToArray();

                    if (tails[0] != tails[1])
                    {
                        walkMap[tails[1]] = MapType.Tail;
                    }
                }
            }
        }

        private void UpdateFoods()
        {
            foreach (Coord food in food)
            {
                if (IsRaceFood(food))
                {
                    walkMap[food] = MapType.RaceFood;
                }
                else
                {
                    walkMap[food] = MapType.Food;
                }
            }
        }

        private bool IsRaceFood(Coord coord)
        {
            if (walkMap[coord + MapMove.Up] == MapType.Head ||
                walkMap[coord + MapMove.Down] == MapType.Head ||
                walkMap[coord + MapMove.Left] == MapType.Head ||
                walkMap[coord + MapMove.Right] == MapType.Head
                )
            {
                return true;
            }

            return false;
        }

        public string GetNextMove(GameRequest request)
        {
            Update(request);

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
                    ApplyScoreMask(x, y, BodyScore, BodyScale);
                    ApplyUnWalkableScore(x, y);
                    break;
                case MapType.MyBody:
                    ApplyScoreMask(x, y, MyBodyScore, MyBodyScale);
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
            List<double> scores = new List<double>();

            Coord head = player.body[0];

            foreach (Coord move in Moves)
            {
                int space = GetLastSpace(head + move);

                if (space < bodySize / 2)
                {
                    scoreMap[head + move] += UnWalkableScore * (1 - space / bodySize);
                }

                scores.Add(scoreMap[head + move]);
            }

            int index = scores.IndexOf(scores.Max());

            return Directions[index];
        }

        private int GetLastSpace(Coord coord, int length = 0)
        {
            switch (walkMap[coord])
            {
                case MapType.Space:
                    break;
                case MapType.Wall:
                case MapType.Head:
                case MapType.WeakHead:
                case MapType.Body:
                case MapType.MyBody:
                    return length;
                case MapType.Tail:
                case MapType.Food:
                case MapType.RaceFood:
                    break;
                default:
                    break;
            }

            if (length >= bodySize / 2)
            {
                return length;
            }

            MapType prev = walkMap[coord];
            walkMap[coord] = MapType.MyBody;

            List<int> spaces = new List<int>();

            foreach (Coord move in Moves)
            {
                spaces.Add(GetLastSpace(coord + move, length + 1));
            }

            walkMap[coord] = prev;

            return spaces.Max();
        }
    }
}
