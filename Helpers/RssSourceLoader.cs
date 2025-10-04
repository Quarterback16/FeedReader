using FeedReader.Models;
using System.Text.Json;

namespace FeedReader.Helpers
{

    public static class RssSourceLoader
    {
        public static List<RssSource> LoadFromJson(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var sources = JsonSerializer.Deserialize<List<RssSource>>(
                json, 
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            return sources ?? new List<RssSource>();
        }
    }
}
