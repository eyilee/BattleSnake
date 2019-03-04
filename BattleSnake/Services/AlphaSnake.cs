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
            Space,
            Wall,
            Head,
            Body,
            Tail,
            Food,
        }

        private static readonly string[] Directions = {
            "up",
            "down",
            "left",
            "right"
        };

        private const int LastSpaceScore = -200;

        private int width;
        private int height;
        private int mapSize;

        private Map<MapType> walkMap;
        private Map<double> scoreMap;

        private const int SpaceScore = 5;
        private const int SpaceScale = 2;

        private const int HeadScore = -24;
        private const int HeadScale = 2;

        private const int BodyScore = -3;
        private const int BodyScale = 1;

        private const int TailScore = 3;
        private const int TailScale = 1;

        private const int FoodScore = 24;
        private const int FoodScale = 2;

        private Snake player;
        private List<Snake> snakes;
        private List<Coord> food;

        public void Init(GameRequest request)
        {
            width = request.board.width + 2;
            height = request.board.height + 2;
            mapSize = request.board.width * request.board.height;

            walkMap = new Map<MapType>(width, height);
            scoreMap = new Map<double>(width, height);
        }

        public void Update(GameRequest request)
        {
            SetPlayer(request.you);
            SetSnakes(request.board.snakes);
            SetFood(request.board.food);

            ClearMaps();

            SetWallsOnMap();
            SetPlayerOnMap();
            SetSnakesOnMap();
            SetFoodOnMap();

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

        private void ClearMaps()
        {
            walkMap.Clear();
            scoreMap.Clear();
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
                walkMap[body] = MapType.Body;
            }

            walkMap[player.body[0]] = MapType.Head;

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

                walkMap[snake.body[0]] = MapType.Head;

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
                walkMap[food] = MapType.Food;
            }
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
                    ApplyScoreMask(x, y, SpaceScore, SpaceScale);
                    break;
                case MapType.Wall:
                    ApplyUnWalkableScore(x, y);
                    break;
                case MapType.Head:
                    ApplyScoreMask(x, y, HeadScore, HeadScale);
                    ApplyUnWalkableScore(x, y);
                    break;
                case MapType.Body:
                    ApplyScoreMask(x, y, BodyScore, BodyScale);
                    ApplyUnWalkableScore(x, y);
                    break;
                case MapType.Tail:
                    ApplyScoreMask(x, y, TailScore, TailScale);
                    break;
                case MapType.Food:
                    ApplyScoreMask(x, y, FoodScore, FoodScale);
                    break;
                default:
                    break;
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
                scoreMap[item.Key] += score * (1 - (item.Value / (scale + 1)));
            }
        }

        private void GetDistance(Coord coord, int scale, Dictionary<Coord, int> distances, int length = 1)
        {
            switch (walkMap[coord])
            {
                case MapType.Space:
                    break;
                case MapType.Wall:
                case MapType.Head:
                case MapType.Body:
                    return;
                case MapType.Tail:
                case MapType.Food:
                    break;
                default:
                    break;
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
                scoreMap[coord] += LastSpaceScore * (1 - (space / (player.body.Count / 2)));
            }
        }

        private int GetLastSpace(Coord coord, int length = 0)
        {
            switch (walkMap[coord])
            {
                case MapType.Space:
                    break;
                case MapType.Wall:
                case MapType.Head:
                case MapType.Body:
                    return length;
                case MapType.Tail:
                case MapType.Food:
                    break;
                default:
                    break;
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
