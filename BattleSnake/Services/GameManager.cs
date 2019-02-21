using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleSnake.Interface;
using BattleSnake.Models;

namespace BattleSnake.Services
{
    public class GameManager
    {
        private static readonly GameManager instance = new GameManager();

        private readonly Dictionary<string, IGame> games = new Dictionary<string, IGame>();

        private GameManager() { }

        public static GameManager Instance {
            get {
                return instance;
            }
        }

        public T CreateGame<T>(string gameId) where T : new()
        {
            if (games.ContainsKey(gameId))
            {
                return default(T);
            }

            T game = new T();

            games.Add(gameId, game as IGame);

            return game;
        }

        public IGame GetGame(string gameId)
        {
            if (games.ContainsKey(gameId))
            {
                return games[gameId];
            }

            return null;
        }

        public void RemoveGame(string gameId)
        {
            if (games.ContainsKey(gameId))
            {
                games.Remove(gameId);
            }
        }
    }
}
