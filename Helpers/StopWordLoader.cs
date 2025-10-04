using System.Text.Json;

namespace FeedReader.Helpers
{
    public static class StopWordsLoader
    {
        public static List<string> Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<string>>(json) 
                ?? new List<string>();
        }
    }
}
