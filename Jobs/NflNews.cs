using FeedReader.Helpers;
using FeedReader.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace FeedReader.Jobs
{
    public static class NflNews
    {
    public static int LatestNewsJob(
            FeederReaderContext settings)
        {
            try
            {
                LogHelper.LogMessage(settings.Logger, "NflNews ...");
                if (settings.StartDateTime == null)
                    return 1;

                var goBackHours = (settings.GoBackHours == null)
                    ? 8
                    : settings.GoBackHours;

                settings.MyRoster ??= false;
                settings.AllNews ??= false;

                if (settings.DropBoxFolder == null)
                    return 1;
                if (settings.NflFeedsFile == null)
                    return 1;
                var feedFile = $"{FolderHelper.JsonFolder(settings.DropBoxFolder)}{settings.NflFeedsFile}";
                var stopWordsFile = $"{FolderHelper.JsonFolder(settings.DropBoxFolder)}stopwords.json";
                var goWordsFile = $"{FolderHelper.JsonFolder(settings.DropBoxFolder)}gowords.json";
                var items = new List<SourceItem>();
                var myItems = new List<SourceItem>();
                var errorFeeds = new List<RssSource>();
                var feeds = FeedCollection.NFL(feedFile);
                //feeds.AddRange(WireHelper.Wires())

                LogHelper.LogMessage(
                    settings.Logger,
                    $"Scanning {feeds.Count} feeds");

                feeds.ForEach(f =>
                {
                    if (settings.MyRoster.Value)
                    {
                        LogHelper.LogMessage(
                            settings.Logger,
                            $"Scanning {f.Source} for my roster");
                        myItems.AddRange(
                            ScanRss(
                                settings.Logger,
                                f,
                                hours: goBackHours,
                                errorFeeds,
                                FeedCollection.StopWords(stopWordsFile),
                                FeedCollection.GoWords(goWordsFile)));
                    }
                    if (settings.AllNews.Value)
                    {
                        LogHelper.LogMessage(
                            settings.Logger,
                            $"Scanning {f.Source} for news");
                        items.AddRange(
                            ScanRss(
                                settings.Logger,
                                f,
                                hours: goBackHours,
                                errorFeeds,
                                FeedCollection.StopWords(stopWordsFile)));
                    }
                });

                if (settings.AllNews.Value)
                {
                    var page = RssHelper.ItemsToSummaryMarkdownTable(
                        items,
                        goBackHours,
                        "[[Latest 7x7ers News]]");
                    LogHelper.LogMessage(
                       settings.Logger,
                       page.PageContents());

                    page.RenderToObsidian(
                        $"{settings.Season}\\Latest News",
                        $"{FolderHelper.GetObsidianNflStemFolder(settings.DropBoxFolder)}");
                }

                if (settings.MyRoster.Value)
                {
                    var page = RssHelper.ItemsToSummaryMarkdownTable(
                        myItems,
                        goBackHours,
                        "[[Latest News]]");
                    LogHelper.LogMessage(
                       settings.Logger,
                       page.PageContents());

                    var targetFile = $@"{FolderHelper.GetObsidianNflStemFolder(settings.DropBoxFolder)}{settings.Season}\\Latest 7x7ers News.md";
                    LogHelper.LogMessage(
                          settings.Logger,
                          $"Rendering to {targetFile}");
                    page.RenderToObsidian(
                        $"{settings.Season}\\Latest 7x7ers News",
                        $"{FolderHelper.GetObsidianNflStemFolder(settings.DropBoxFolder)}");
                }

                if (errorFeeds.Any())
                {
                    errorFeeds.ForEach(
                        ef => LogHelper.LogMessage(
                            settings.Logger,
                            $"Error with feed {ef.Source} {ef.FeedUrl}"));
                }

                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(
                    settings.Logger,
                    $"Exception {ex.Message}");
                throw;
            }
        }

        public static List<SourceItem> ScanRss(
            ILogger? logger,
            RssSource source,
            int? hours,
            List<RssSource> errorFeeds,
            List<string> stopWords,
            List<string> goWords = null)
        {
            var items = new List<SourceItem>();
            if (!source.Enabled)
                return items;
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                var content = client.GetStringAsync(source.FeedUrl).Result;

                CodeHollow.FeedReader.Feed feed = null;
                try
                {
                    feed = CodeHollow.FeedReader.FeedReader.ReadFromString(content);
                }
                catch (Exception)
                {
                    // content is not a feed, so try to parse it as HTML
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(content);

                    var feedNode = htmlDoc.DocumentNode.SelectSingleNode("//link[@type='application/rss+xml' or @type='application/atom+xml']");
                    if (feedNode != null)
                    {
                        var feedUrl = feedNode.GetAttributeValue("href", "");
                        if (!string.IsNullOrWhiteSpace(feedUrl))
                        {
                            // if the url is relative, combine it with the base url
                            if (feedUrl.StartsWith("/"))
                            {
                                var uri = new Uri(source.FeedUrl);

                                feedUrl = $"{uri.Scheme}://{uri.Host}{feedUrl}";
                            }
                            var readerTask = CodeHollow.FeedReader.FeedReader.ReadAsync(feedUrl);
                            readerTask.ConfigureAwait(false);
                            feed = readerTask.Result;
                        }
                    }
                }

                if (feed != null)
                {
                    LogHelper.LogMessage(
                        logger,
                        $"feed for {source.Source} has {feed.Items.Count} items");
                    foreach (var item in feed.Items)
                    {
                        if (goWords != null && !ItemsHasGoWords(item, goWords))
                            continue;
                        if (ItemHasStopWords(item, stopWords))
                            continue;
                        if (InRange(item.PublishingDate, hours))
                            items.Add(new SourceItem { Item = item, Source = source.Source });
                    }
                    source.Items = items;
                }
                else
                {
                    LogHelper.LogMessage(
                        logger,
                        $"Could not find a feed for {source.Source}");
                    errorFeeds.Add(source);
                }
                return items;
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(
                    logger,
                    $@"An error occurred: {ex.Message}{Environment.NewLine}{ex.InnerException}");
                errorFeeds.Add(source);
                return items;
            }
        }

        private static bool ItemsHasGoWords(
            CodeHollow.FeedReader.FeedItem item,
            List<string> goWords)
        {
            if (string.IsNullOrEmpty(item.Title))
                return false;
            else
            {
                if (goWords.Any(w => item.Title.Contains(w, StringComparison.CurrentCulture)))
                    return true;
            }
            if (string.IsNullOrEmpty(item.Description))
                return false;
            else
            {
                if (goWords.Any(w => item.Description.Contains(w, StringComparison.CurrentCulture)))
                    return true;
            }
            return false;
        }

        private static bool ItemHasStopWords(
            CodeHollow.FeedReader.FeedItem item,
            List<string> stopwords)
        {
            if (item == null)
                return false;
            if (string.IsNullOrEmpty(item.Title))
                return false;
            else
            {
                if (stopwords.Any(w => item.Title.Contains(w, StringComparison.CurrentCulture)))
                    return true;
            }
            if (string.IsNullOrEmpty(item.Description))
                return false;
            else
            {
                if (stopwords.Any(w => item.Description.Contains(w, StringComparison.CurrentCulture)))
                    return true;
            }
            return false;
        }

        private static bool InRange(
            DateTime? publishingDate,
            int? hours)
        {
            if (publishingDate == null)
                return false;

            TimeZoneInfo localZone = TimeZoneInfo.Local;
            DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(
                publishingDate.Value,
                localZone);
            var ageOfItem = DateTime.Now - localDate;
            if (ageOfItem < new TimeSpan(hours.Value, 0, 0))
                return true;
            return false;
        }
    }


}
