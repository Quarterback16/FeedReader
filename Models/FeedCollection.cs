using FeedReader.Helpers;

namespace FeedReader.Models
{
    public static class FeedCollection
    {
        public static List<string> StopWords(string filePath) => 
            StopWordsLoader.Load(filePath);

        public static List<RssSource> NFL(string filePath) => 
            RssSourceLoader.LoadFromJson(filePath)
                           .Where(feed => feed.Enabled)
                           .ToList();

        public static List<string> GoWords(string filePath) =>
            StopWordsLoader.Load(filePath);

    }
}
