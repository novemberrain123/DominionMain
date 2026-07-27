using System.Text.Json;
using Dominion.API.Config;

namespace Dominion.API.Dominion.Game
{
    public class GameModeLoader
    {
        public GameConfig Load(string path)
        {
            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<GameConfig>(json, JsonConfig.Options)!;
        }
    }
}
