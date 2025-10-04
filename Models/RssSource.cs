namespace FeedReader.Models
{
    public class RssSource
    {
        public string Source { get; set; }
        public string FeedUrl { get; set; }
        public string Comment { get; set; }
        public bool Enabled { get; set; }
        public List<SourceItem> Items { get; set; }
    }
}
