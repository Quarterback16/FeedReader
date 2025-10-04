using WikiPages;

namespace FeedReader.Helpers
{
    public static class ClockHelper
    {
        public static WikiPage ClocksToMarkdownTable(
            List<(string Name, TimeZoneInfo Zone)> clocks)
        {
            var page = new WikiPage();
            page.AddLine("=== start-multi-column: Clocks");
            page.AddBlankLine();
            page.AddLine("```column-settings");
            page.AddLine($"number of columns: {clocks.Count + 1}");
            page.AddLine("```");

            AddClock("Local", TimeZoneInfo.Local, page);

            foreach (var clock in clocks)
            {
                AddClock(clock.Name, clock.Zone, page);
            }

            page.AddBlankLine();
            page.AddLine("=== end-multi-column");
            return page;
        }

        static void AddClock(
            string name, 
            TimeZoneInfo zone, 
            WikiPage page)
        {
            var theTimeInTheZone = TimeZoneInfo.ConvertTime(DateTime.Now, zone);
            var theTime = new WikiTable();
            theTime.AddColumn(name);
            theTime.AddRows(1);
            theTime.AddCell(1, 0, theTimeInTheZone.ToString("ddd hh:mm tt"));
            page.AddBlankLine();
            page.AddTable(theTime);
            page.AddBlankLine();
            page.AddLine("=== end-column ===");
        }
    }
}