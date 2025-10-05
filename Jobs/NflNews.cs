using FeedReader.Helpers;
using FeedReader.Models;
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
                //feeds.AddRange(WireHelper.Wires());

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

                    var targetFile = $@"{
                            FolderHelper.GetObsidianNflStemFolder(settings.DropBoxFolder)
                            }{
                            settings.Season
                            }\\Latest 7x7ers News.md";
                    LogHelper.LogMessage(
                          settings.Logger,
                          $"Rendering to {targetFile}");
                    page.RenderToObsidian(
                        $"{settings.Season}\\Latest 7x7ers News",
                        $"{FolderHelper.GetObsidianNflStemFolder(settings.DropBoxFolder)}");
                }

                if ( errorFeeds.Any())
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
                // following 4 lines to add a User-Agent header to avoid 403 errors
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                var html = client.GetStringAsync(source.FeedUrl);
                var urls = CodeHollow.FeedReader.FeedReader.ParseFeedUrlsFromHtml(html.Result);
                //var urlsTask = CodeHollow.FeedReader.FeedReader.GetFeedUrlsFromUrlAsync(source.FeedUrl);
                //var urls = urlsTask.Result;

                string feedUrl;
                if (urls == null || urls.Count() < 1)
                    feedUrl = source.FeedUrl;
                else if (urls.Count() == 1)
                    feedUrl = urls.First().Url;
                else if (urls.Count() == 2)
                    // if 2 urls, then its usually a feed and a comments feed, 
                    // so take the first per default
                    feedUrl = urls.First().Url;
                else
                {
                    Console.WriteLine("Found multiple feed, please choose:");
                    foreach (var feedurl in urls)
                    {
                        Console.WriteLine($"{feedurl.Title} - {feedurl.Url}");
                    }
                    return items;
                }

                try
                {
                    var readerTask = CodeHollow.FeedReader.FeedReader.ReadAsync(feedUrl);
                    readerTask.ConfigureAwait(false);

                    foreach (var item in readerTask.Result.Items)
                    {
                        if (goWords != null && !ItemsHasGoWords(item, goWords))
                            continue;
                        if (ItemHasStopWords(item, stopWords))
                            continue;
                        if (InRange(item.PublishingDate, hours))
                            items.Add(new SourceItem { Item = item, Source = source.Source });
                    }
                    source.Items = items;
                    return items;
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessage(
                        logger,
                        $"Error Getting feed {source.Source} {ex.Message}");
                    errorFeeds.Add(source);
                    //Console.WriteLine(
                    //  $@"An error occurred: {ex.InnerException?.Message}{Environment.NewLine}{ex.InnerException}");
                    return items;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(
                    logger,
                    $@"An error occurred: {ex.InnerException?.Message}{Environment.NewLine}{ex.InnerException}");
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
