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

        private static readonly Coords[] Moves = {
            Coords.Up,
            Coords.Down,
            Coords.Left,
            Coords.Right
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

            walkMap = new Map<MapType>(width, height);
            scoreMap = new Map<double>(width, height);

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
            coords.ForEach(x => x += Coords.Identity);
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
            head = player.body[0];
            health = player.health;
            bodySize = player.body.Count;

            foreach (Coords body in player.body)
            {
                walkMap[body] = MapType.Body;
            }

            if (player.body.Count > 2)
            {
                Coords[] tails = player.body.TakeLast(2).ToArray();

                if (tails[0] != tails[1])
                {
                    walkMap[tails[1]] = MapType.Tail;
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
                    walkMap[body] = MapType.Body;
                }

                if (rival.body.Count >= player.body.Count)
                {
                    walkMap[rival.body[0]] = MapType.Head;
                }
                else
                {
                    walkMap[rival.body[0]] = MapType.WeakHead;
                }

                if (rival.body.Count >= 2)
                {
                    Coords[] tails = rival.body.TakeLast(2).ToArray();

                    if (tails[0] != tails[1])
                    {
                        walkMap[tails[1]] = MapType.Tail;
                    }
                }
            }
        }

        private void UpdateFood()
        {
            foreach (Coords food in foods)
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

        private bool IsRaceFood(Coords coords)
        {
            if (walkMap[coords + Coords.Up] == MapType.Head ||
                walkMap[coords + Coords.Down] == MapType.Head ||
                walkMap[coords + Coords.Left] == MapType.Head ||
                walkMap[coords + Coords.Right] == MapType.Head
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
            List<double> scores = new List<double>();

            foreach (Coords move in Moves)
            {
                int space = GetLastSpace(head + move);

                if (space <= bodySize / 2)
                {
                    scoreMap[head + move] += UnWalkableScore * (1 - space / (bodySize / 2));
                }

                scores.Add(scoreMap[head + move]);
            }

            int index = scores.IndexOf(scores.Max());

            return Directions[index];
        }

        private int GetLastSpace(Coords coords, int length = 0)
        {
            switch (walkMap[coords])
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

            MapType prev = walkMap[coords];
            walkMap[coords] = MapType.Body;

            Coords[] tail = player.body.TakeLast(2).ToArray();
            MapType[] prevTail = { walkMap[tail[0]], walkMap[tail[1]] };
            walkMap[tail[0]] = MapType.Tail;
            walkMap[tail[1]] = MapType.Space;

            List<int> spaces = new List<int>();

            foreach (Coords move in Moves)
            {
                spaces.Add(GetLastSpace(coords + move, length + 1));
            }

            walkMap[coords] = prev;
            walkMap[tail[0]] = prevTail[0];
            walkMap[tail[1]] = prevTail[1];

            return spaces.Max();
        }
    }
}
