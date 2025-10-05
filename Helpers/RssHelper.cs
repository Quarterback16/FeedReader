using FeedReader.Models;
using WikiPages;
using Humanizer;
using HtmlAgilityPack;

namespace FeedReader.Helpers
{
    public static class RssHelper
    {
        public static WikiPage ItemsToSummaryMarkdownTable(
            List<SourceItem> items,
            int? goBackHours,
            string link)
        {
            var page = new WikiPageWithTable();
            page.AddLine("---");
            page.AddLine("cssclasses: purpleRed,t-c,illusion");
            page.AddLine("---");
            page.AddHeading($"Latest NFL News - {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}");
            page.AddLine(ClockHeader());
            page.AddLine($"- {link}");
            page.AddBlankLine();
            page.AddHeading($"{items.Count} items from the Last {goBackHours} Hour(s)", 3);

            page.AddBlankLine();
            page.Table.AddColumn("When");
            page.Table.AddColumn("Source");
            page.Table.AddColumn("Title");
            page.Table.AddColumn("Desc");
            page.Table.AddColumn("Link");
            page.Table.AddRows(items.Count);

            var row = 0;
            items.OrderByDescending(i => i.Item.PublishingDate).ToList().ForEach(i =>
            {
                page.Table.AddCell(++row, 0, i.Item.PublishingDate.Humanize().Trim());
                page.Table.AddCell(row, 1, Fix(i.Source));
                page.Table.AddCell(row, 2, Fix(i.Item.Title));
                page.Table.AddCell(row, 3, StripImgSize(Fix(i.Item.Description)));
                page.Table.AddCell(row, 4, FixLink(i.Item.Link));
            });
            page.AddTable(page.Table);
            return page;
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


        static DateTime LocalTime(
            DateTime? publishingDate)
        {
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(
                publishingDate.Value,
                localZone);
            return localDate;
        }

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
