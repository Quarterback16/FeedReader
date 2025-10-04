using FeedReader.Models;

namespace FeedReader.Helpers
{
    public static class WireHelper
    {
        public static IEnumerable<RssSource> Wires()
        {
            var wires = new string[]
            {
            "Bengals",
            "Steelers",
            "Browns",
            "Ravens",
            "Jaguars",
            "Texans",
            "Colts",
            "Broncos",
            "Chargers",
            "Chiefs",
            "Raiders",
            "Commanders",
            "TheEagles",
            "Giants",
            "Cowboys",
            "Bears",
            "Lions",
            "Packers",
            "Vikings",
            "Bucs",
            "TheFalcons",
            "Saints",
            "Panthers",
            "Niners",
            "Cards",
            "TheRams",
            "Seahawks",
            "Titans",
            "Bills",
            "Jets",
            "Patriots",
            "Dolphins",
            };
            var list = new List<RssSource>();
            wires.ToList().ForEach(w =>
            {
                list.Add(
                    new RssSource
                    {
                        Source = $"{w} wire",
                        Comment = "from usa today",
                        FeedUrl = $"https://{w.ToLower()}wire.usatoday.com/feed/",
                        Enabled = true
                    });
            });
            return list;
        }
    }
}
