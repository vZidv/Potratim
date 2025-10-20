using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;
using Potratim.ViewModel;

namespace src.Services
{
    public interface IGameService
    {
        public Task<Game?> GetGameAsync(Guid id);
        public Task<Game?> GetGameAsync(string id);

        public Task<Game> CreateGameAsync(CreateGameViewModel model);

        public Task<Game> UpdateGameAsync(EditGameViewModel model);

        public Task<bool> DeleteGameAsync(Guid id);

        public Task<List<GameViewModel>> GetSimilarGamesAsync(string id, int count);
        public Task<List<GameViewModel>> GetSomeGamesAsync(int count);

        public GameViewModel GameToGameViewModel(Game game);

    }
}