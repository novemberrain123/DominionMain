using Dominion.API.Config;
using Dominion.API.Dominion.Serialization;
using System.Text.Json;

namespace Dominion.API.Dominion.Game
{
    public class GameModeLoader
    {
        public GameConfig Load(string path)
        {
            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<GameConfig>(
                json,
                JsonConfig.Options
            )!;
        }

        public List<GameModeDto> GetAvailableModes()
        {
            var modesDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "Content",
                "Modes"
            );

            var modes = new List<GameModeDto>();

            foreach (var path in Directory.GetFiles(modesDirectory, "*.json"))
            {
                var config = Load(path);

                modes.Add(new GameModeDto
                {
                    Name = config.Name,
                    DisplayName = config.DisplayName,
                    Description = config.Description
                });
            }

            return modes;
        }
    }
}
