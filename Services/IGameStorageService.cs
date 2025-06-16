using System.Collections.Generic;
using ConnectDotsGame.Models;

namespace ConnectDotsGame.Services
{
    public interface IGameStorageService
    {
        void LoadProgress(List<Level> levels);
        void SaveProgress(List<Level> levels);
    }
} 