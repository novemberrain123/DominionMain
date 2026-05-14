using System.Text.Json;

namespace Dominion.Config
{
    public static class JsonConfig
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
