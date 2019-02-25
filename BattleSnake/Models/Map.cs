﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Models
{
    public class Map<T>
    {
        private T[,] values;

        public Map(int width, int height)
        {
            values = new T[width, height];
        }

        public void Clear()
        {
            Array.Clear(values, 0, values.Length);
        }

        public T Min()
        {
            return values.Cast<T>().Min();
        }

        public T Max()
        {
            return values.Cast<T>().Max();
        }

        public T this[int x, int y] {
            get { return values[x, y]; }
            set { values[x, y] = value; }
        }

        public T this[Coord coords] {
            get { return values[coords.x, coords.y]; }
            set { values[coords.x, coords.y] = value; }
        }
    }
}
