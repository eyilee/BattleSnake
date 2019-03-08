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
        public string NextMove { get; set; }

        private enum MapType
        {
            None = 0,
            Space = 1,
            Wall = 2,
            Head = 4,
            PlayerHead = 8,
            WeakHead = 16,
            Body = 32,
            PlayerBody = 64,
            Tail = 128,
            Food = 256,
            RaceFood = 512,
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
        private const double IdentityScore = 3;

        private const double LastSpaceScore = -200;

        private const double SpaceScore = 3;
        private const int SpaceScale = 3;

        private const double HeadScore = -30;
        private const int HeadScale = 1;

        private const double PlayerHeadScore = 3;
        private const int PlayerHeadScale = 3;

        private const double WeakHeadScore = 3;
        private const int WeakHeadScale = 3;

        private const double BodyScore = -1;
        private const int BodyScale = 1;

        private const double PlayerBodyScore = -1;
        private const int PlayerBodyScale = 1;

        private const double TailScore = 3;
        private const int TailScale = 3;

        private double FoodScore = 12;
        private int FoodScale = 3;

        private const double RaceFoodScore = -15;
        private const int RaceFoodScale = 1;

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

            ClearMaps();

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

        private void ClearMaps()
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
                FoodScore = 36;
                FoodScale = 5;
            }
            else if (player.health < 60)
            {
                FoodScore = 24;
                FoodScale = 4;
            }
            else
            {
                FoodScore = 12;
                FoodScale = 3;
            }

            foreach (Snake snake in snakes)
            {
                if (player.body.Count <= snake.body.Count)
                {
                    FoodScore += 12;
                    FoodScale += 1;
                    break;
                }
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
                    break;
                case MapType.Head:
                    ApplyScoreMask(x, y, HeadScore, HeadScale);
                    break;
                case MapType.PlayerHead:
                    ApplyScoreMask(x, y, PlayerHeadScore, PlayerHeadScale);
                    break;
                case MapType.WeakHead:
                    ApplyScoreMask(x, y, WeakHeadScore, WeakHeadScale);
                    break;
                case MapType.Body:
                    ApplyScoreMask(x, y, BodyScore, BodyScale);
                    break;
                case MapType.PlayerBody:
                    ApplyScoreMask(x, y, PlayerBodyScore, PlayerBodyScale);
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

            if ((walkMap[x, y] & MapType.UnWalkable) != MapType.None)
            {
                ApplyUnWalkableScore(x, y);
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
