using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Models
{
    public class Map<T>
    {
        private T[,] map;

        public int Width { get; }
        public int Height { get; }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            map = new T[width, height];
        }

        public void Clear()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    map[i, j] = default(T);
                }
            }
        }

        public T this[int x, int y] {
            get { return map[x, y]; }
            set { map[x, y] = value; }
        }

        public T this[Coord coord] {
            get { return map[coord.x, coord.y]; }
            set { map[coord.x, coord.y] = value; }
        }
    }
}
