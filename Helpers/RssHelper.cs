using FeedReader.Models;
using HtmlAgilityPack;
using Humanizer;
using WikiPages;

namespace FeedReader.Helpers
{
    public static class RssHelper
    {
        public static WikiPage ItemsToSummaryMarkdownTable(
            List<SourceItem> items,
            int? goBackHours,
            string link)
        {
            var titles = new List<string>();
            var nonDuplicates = items
                .Where(i => !DuplicateItem(i.Item.Title, titles))
                .OrderByDescending(i => i.Item.PublishingDate)
                .ToList();

            var page = new WikiPageWithTable();
            page.AddLine("---");
            page.AddLine("cssclasses: purpleRed,t-c,illusion");
            page.AddLine("---");
            page.AddHeading(
                $"Latest NFL News - {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}");
            page.AddLine(ClockHeader());
            page.AddLine($"- {link}");
            page.AddBlankLine();
            page.AddHeading(
                $"{nonDuplicates.Count} items from the Last {goBackHours} Hour(s)", 
                level: 3);

            page.AddBlankLine();
            page.Table.AddColumn("When");
            page.Table.AddColumn("Source");
            page.Table.AddColumn("Title");
            page.Table.AddColumn("Desc");
            page.Table.AddColumn("Link");
            page.Table.AddRows(nonDuplicates.Count);

            int localRow = 0;
            nonDuplicates.ForEach(i =>
            {
                page.Table.AddCell(++localRow, 0, i.Item.PublishingDate.Humanize().Trim());
                page.Table.AddCell(localRow, 1, Fix(i.Source));
                page.Table.AddCell(localRow, 2, Fix(i.Item.Title));
                page.Table.AddCell(localRow, 3, StripImgSize(Fix(i.Item.Description)));
                page.Table.AddCell(localRow, 4, FixLink(i.Item.Link));
            });

            page.AddTable(page.Table);
            return page;
        }

        private static bool DuplicateItem(
            string title,
            List<string> titles)
        {
            if (titles.Contains(title))
                return true;
            titles.Add(title);
            return false;
        }

        static string ClockHeader()
        {
            var clocks = new List<(string Name, TimeZoneInfo Zone)>
            {
                ("New York", TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time")),
                ("Chicago", TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time")),
                ("San Francisco", TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")),
                ("London", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")),
                ("Rome", TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"))
            };

            var banner = ClockHelper.ClocksToMarkdownTable(clocks);
            return banner.PageContents();
        }

        static string FixLink(string link) =>

            string.IsNullOrEmpty(link) ? string.Empty : $"[link]({Fix(link)})";


        static string Fix(string description) =>

            string.IsNullOrEmpty(description)
                ? string.Empty
                : description.Replace("\r", "").Replace("\n", "").Trim();

        static string StripImgSize(string text)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(text);

            var nodes = document.DocumentNode.SelectNodes("//img");
            if (nodes == null)
                return text;

            foreach (var imgNode in nodes)
            {
                imgNode.Attributes.Remove("width");
                imgNode.Attributes.Remove("height");
            }

            string updatedHtml = document.DocumentNode.OuterHtml;
            return updatedHtml;
        }
    }
}
