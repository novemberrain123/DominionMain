using System.Text.Json;
using Dominion.Config;

namespace Dominion.Dominion.Game
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
