using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleSnake.Services
{
    public class GameManager
    {
        private static readonly GameManager instance = new GameManager();

        private GameManager() { }

        public static GameManager Instance {
            get {
                return instance;
            }
        }
    }
}
