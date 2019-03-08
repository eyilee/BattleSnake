﻿using BattleSnake.Interface;
using BattleSnake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Services
{
    public class AlphaSnake : IGame
    {
        public string NextMove { get; set; }

        private enum MapType
        {
            None = 0,
            Space = 0b0000_0000_0001,
            Wall = 0b0000_0000_0010,
            Head = 0b0000_0000_0100,
            PlayerHead = 0b0000_0000_1000,
            WeakHead = 0b0000_0001_0000,
            Body = 0b0000_0010_0000,
            PlayerBody = 0b0000_0100_0000,
            Tail = 0b0000_1000_0000,
            Food = 0b0001_0000_0000,
            RaceFood = 0b0010_0000_0000,
            Walkable = Space | Tail | Food | RaceFood,
            UnWalkable = Wall | Head | PlayerHead | WeakHead | Body | PlayerBody
        }

        private static readonly string[] Directions = {
            "up",
            "down",
            "left",
            "right"
        };

        // TODO: modify values by board state
        private readonly double[] ScaleFactor = { 1, 2, 10 / 3, 5, 37 / 5, 56 / 6 };

        private const int IdentityScale = 3;
        private const double IdentityScore = 5;

        private const double LastSpaceScore = -200;

        private const int WalkableScale = IdentityScale;
        private const double WalkableScore = IdentityScore;

        private const int HeadScale = 1;
        private const double HeadScore = LastSpaceScore;

        private const int WeakHeadScale = 1;
        private const double WeakHeadScore = IdentityScore;

        private const int PlayerBodyScale = 1;
        private const double PlayerBodyScore = IdentityScore;

        private int FoodScale;
        private double FoodScore;

        private const int RaceFoodScale = 0;
        private const double RaceFoodScore = LastSpaceScore;

        private int width;
        private int height;
        private int mapSize;

        private Map<MapType> walkMap;
        private Map<double> scoreMap;

        private Snake player;
        private List<Snake> snakes;
        private List<Coord> food;

        public void Init(GameRequest request)
        {
            width = request.board.width + 2;
            height = request.board.height + 2;
            mapSize = request.board.width * request.board.height;

            walkMap = new Map<MapType>(width, height, MapType.Space);
            scoreMap = new Map<double>(width, height, 0);
        }

        public void Update(GameRequest request)
        {
            SetPlayer(request.you);
            SetSnakes(request.board.snakes);
            SetFood(request.board.food);

            ResetMaps();

            SetWallsOnMap();
            SetPlayerOnMap();
            SetSnakesOnMap();
            SetFoodOnMap();

            SetOptions();

            CalculateScoreMap();

            SetNextMove();
        }

        private void SetPlayer(Snake player)
        {
            for (int i = 0; i < player.body.Count; i++)
            {
                player.body[i] += MapMove.Identity;
            }

            this.player = player;
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

        private void SetFood(List<Coord> food)
        {
            for (int i = 0; i < food.Count; i++)
            {
                food[i] += MapMove.Identity;
            }

            this.food = food;
        }

        private void ResetMaps()
        {
            walkMap.Reset();
            scoreMap.Reset();
        }

        private void SetWallsOnMap()
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

        private void SetPlayerOnMap()
        {
            foreach (Coord body in player.body)
            {
                walkMap[body] = MapType.PlayerBody;
            }

            walkMap[player.body[0]] = MapType.PlayerHead;

            if (player.body.Count >= 3)
            {
                Coord[] tails = player.body.TakeLast(2).ToArray();

                if (tails[0] != tails[1])
                {
                    walkMap[tails[1]] = MapType.Tail;
                }
            }
        }

        private void SetSnakesOnMap()
        {
            foreach (Snake snake in snakes)
            {
                foreach (Coord body in snake.body)
                {
                    walkMap[body] = MapType.Body;
                }

                if (snake.body.Count < player.body.Count)
                {
                    walkMap[snake.body[0]] = MapType.WeakHead;
                }
                else
                {
                    walkMap[snake.body[0]] = MapType.Head;
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

        private void SetFoodOnMap()
        {
            foreach (Coord food in food)
            {
                if (walkMap[food + MapMove.Up] == MapType.Head ||
                    walkMap[food + MapMove.Down] == MapType.Head ||
                    walkMap[food + MapMove.Left] == MapType.Head ||
                    walkMap[food + MapMove.Right] == MapType.Head)
                {
                    walkMap[food] = MapType.RaceFood;
                }
                else
                {
                    walkMap[food] = MapType.Food;
                }
            }
        }

        // TODO: modify values by board state
        private void SetOptions()
        {
            if (player.health < 30)
            {
                FoodScale = IdentityScale + 2;
            }
            else if (player.health < 60)
            {
                FoodScale = IdentityScale;
            }
            else
            {
                FoodScale = IdentityScale - 2;
            }

            foreach (Snake snake in snakes)
            {
                if (player.body.Count <= snake.body.Count)
                {
                    FoodScale += 1;
                    break;
                }
            }

            FoodScore = IdentityScore * ScaleFactor[FoodScale] * (FoodScale + 1) / FoodScale;
        }

        private void CalculateScoreMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CalculateMapScoreAt(x, y);
                }
            }

            CalculateLastSpaceScore();
        }

        private void CalculateMapScoreAt(int x, int y)
        {
            switch (walkMap[x, y])
            {
                case MapType.Space:
                case MapType.Wall:
                    break;
                case MapType.Head:
                    ApplyScoreMask(x, y, HeadScore, HeadScale);
                    break;
                case MapType.PlayerHead:
                    break;
                case MapType.WeakHead:
                    ApplyScoreMask(x, y, WeakHeadScore, WeakHeadScale);
                    break;
                case MapType.Body:
                    break;
                case MapType.PlayerBody:
                    ApplyScoreMask(x, y, PlayerBodyScore, PlayerBodyScale);
                    break;
                case MapType.Tail:
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

            if ((walkMap[x, y] & MapType.UnWalkable) != MapType.None)
            {
                ApplyUnWalkableScore(x, y);
            }
            else
            {
                ApplyWalkableScore(x, y);
            }
        }

        private void ApplyScoreMask(int x, int y, double score, int scale)
        {
            Dictionary<Coord, int> distances = new Dictionary<Coord, int>();

            Coord coord = new Coord { x = x, y = y };
            distances.Add(coord, 0);

            GetDistance(coord + MapMove.Up, scale, distances);
            GetDistance(coord + MapMove.Down, scale, distances);
            GetDistance(coord + MapMove.Left, scale, distances);
            GetDistance(coord + MapMove.Right, scale, distances);

            foreach (KeyValuePair<Coord, int> item in distances)
            {
                scoreMap[item.Key] += score * (1 - ((double)item.Value / (scale + 1)));
            }
        }

        private void GetDistance(Coord coord, int scale, Dictionary<Coord, int> distances, int length = 1)
        {
            if ((walkMap[coord] & MapType.UnWalkable) != MapType.None)
            {
                return;
            }

            if (length > scale)
            {
                return;
            }

            if (distances.ContainsKey(coord))
            {
                if (length < distances[coord])
                {
                    distances[coord] = length;
                }
            }
            else
            {
                distances.Add(coord, length);
            }

            GetDistance(coord + MapMove.Up, scale, distances, length + 1);
            GetDistance(coord + MapMove.Down, scale, distances, length + 1);
            GetDistance(coord + MapMove.Left, scale, distances, length + 1);
            GetDistance(coord + MapMove.Right, scale, distances, length + 1);
        }

        private void ApplyWalkableScore(int x, int y)
        {
            ApplyScoreMask(x, y, WalkableScore, WalkableScale);
        }

        private void ApplyUnWalkableScore(int x, int y)
        {
            scoreMap[x, y] = double.NegativeInfinity;
        }

        private void CalculateLastSpaceScore()
        {
            Coord head = player.body[0];

            CalculateLastSpaceScoreAt(head + MapMove.Up);
            CalculateLastSpaceScoreAt(head + MapMove.Down);
            CalculateLastSpaceScoreAt(head + MapMove.Left);
            CalculateLastSpaceScoreAt(head + MapMove.Right);
        }

        private void CalculateLastSpaceScoreAt(Coord coord)
        {
            int space = GetLastSpace(coord);

            if (space < player.body.Count / 2)
            {
                scoreMap[coord] += LastSpaceScore * (1 - ((double)space / (player.body.Count / 2)));
            }
        }

        private int GetLastSpace(Coord coord, int length = 0)
        {
            if ((walkMap[coord] & MapType.UnWalkable) != MapType.None)
            {
                return length;
            }

            if (length >= player.body.Count / 2)
            {
                return length;
            }

            MapType prev = walkMap[coord];
            walkMap[coord] = MapType.Body;

            List<int> spaces = new List<int>
            {
                GetLastSpace(coord + MapMove.Up, length + 1),
                GetLastSpace(coord + MapMove.Down, length + 1),
                GetLastSpace(coord + MapMove.Left, length + 1),
                GetLastSpace(coord + MapMove.Right, length + 1)
            };

            walkMap[coord] = prev;

            return spaces.Max();
        }

        private void SetNextMove()
        {
            Coord head = player.body[0];

            List<double> scores = new List<double>
            {
                scoreMap[head + MapMove.Up],
                scoreMap[head + MapMove.Down],
                scoreMap[head + MapMove.Left],
                scoreMap[head + MapMove.Right]
            };

            int index = scores.IndexOf(scores.Max());

            NextMove = Directions[index];
        }
    }
}
