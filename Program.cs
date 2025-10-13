using LanguageExt;
using static LanguageExt.Prelude;
using static System.Console;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Configuration;
using FeedReader.Jobs;
using FeedReader.Helpers;

namespace FeedReader
{
    internal partial class Program
    {
        public ILogger? Logger { get; set; }

        static void Main()
        {
            _ = GetSettings()
                .Bind(x => SetupLogging(x))
                .Bind(x => DumpSettings(x))
                .Match(
                    Some: s => DoTheJobs(s),
                    None: () => None);
        }

        private static Option<FeederReaderContext> SetupLogging(
            FeederReaderContext context)
        {
            if (string.IsNullOrEmpty(context.LogFile))
            {
                WriteLine("No LogFile path found");
                return None;
            }
            var logFile = $"{context.LogFile}-{DateTime.Now:yyyy-MM-dd}.log";
            context.StreamWriter = new(logFile, append: true);
            using ILoggerFactory factory = LoggerFactory.Create(
                builder =>
                {
                    // output to the console
                    builder.AddSimpleConsole(
                        options =>
                        {
                            options.IncludeScopes = false;
                            options.SingleLine = true;
                            options.TimestampFormat = "HH:mm:ss ";
                            options.ColorBehavior = LoggerColorBehavior.Enabled;
                        });
                    // and output to a text file
                    builder.AddProvider(
                        new CustomFileLoggerProvider(
                            context.StreamWriter));
                });
            context.Logger = factory.CreateLogger<Program>();
            return context;
        }

        private static Option<FeederReaderContext> DoTheJobs(
            FeederReaderContext context)
        {
            if (context.Logger == null)
            {
                WriteLine("No logger - aborting mission");
                return None;
            }
            // connected now loop around until knock off time
            while (DateTime.Now.TimeOfDay < ToTimeOfDay(context.KnockOffTime))
            {
                var nErrors = 0;
                nErrors = NflNews.LatestNewsJob(context);

                if (nErrors > 0)
                {
                    LogHelper.LogMessage(
                        context.Logger,
                        $"There are {nErrors} errors atm");
                }
                else
                    LogHelper.LogMessage(
                        context.Logger,
                        $@"No errors found at {DateTime.Now:u} {DaysSince(context)} days without incident");
                SleepForMinutes(context, 60);
            }
            LogHelper.LogMessage(
                context.Logger,
                $"Knock off time {context.KnockOffTime} : FeedReader shutting down...");

            return None;
        }

        private static void SleepForMinutes(
            FeederReaderContext context,
            int minutes)
        {
            var msToWait = context.Frequency == null
                    ? 60000 * minutes
                    : context.Frequency.Value * 1000 * minutes;
            LogHelper.LogMessage(context.Logger, $"Restart in {msToWait / 60000} minutes");
            Thread.Sleep(msToWait);
        }

        private static TimeSpan ToTimeOfDay(
            string? knockOffTime) =>

                (knockOffTime == null)
                    ? TimeSpan.Zero
                    : DateTime.Parse(knockOffTime).TimeOfDay;


        private static string SelectConnectionString(
            FeederReaderContext settings) =>

                settings.ConnectTo == "Prod"
                    ? settings?.ConnectionStrings?.Prod ?? string.Empty
                    : settings?.ConnectionStrings?.Dev ?? string.Empty;


        private static Option<FeederReaderContext> GetSettings()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(
                    "appSettings.json",
                    optional: false,
                    reloadOnChange: true);

            var config = builder.Build();

            if (config != null)
            {
                var settings = config.GetSection("Settings")
                    .Get<FeederReaderContext>(); // needs Configuration.Binder NuGet

                if (settings != null)
                {
                    //  Use default values if none found
                    if (settings.StartDateTime == null)
                        settings.StartDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Local);

                    if (string.IsNullOrEmpty(settings.ConnectTo))
                        settings.ConnectTo = "Dev";

                    settings.Beep ??= false;

                    settings.Stats ??= false;

                    if (!settings.Frequency.HasValue)
                        settings.Frequency = 60;

                    return Some(settings);
                }
                else
                    return None;
            }
            else
                return None;
        }

        private static Option<FeederReaderContext> DumpSettings(
            FeederReaderContext context)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                DateTime buildDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
                    .AddDays(version.Build)
                    .AddSeconds(version.Revision * 2);
                LogHelper.LogSettingMessage(context.Logger, "Build        ", buildDate.ToString("yyyy-MM-dd"));
            }

            var startDate = context.StartDateTime?.ToString("yyyy'-'MM'-'dd' 'hh':'mm");

            LogHelper.LogSettingMessage(context.Logger, "Starting", startDate);
            WriteSettings(context.Logger, "AppNames  ", context.AppNames);
            LogHelper.LogSettingMessage(context.Logger, "ConnectTo      ", context.ConnectTo);
            LogHelper.LogSettingMessage(context.Logger, "Conn str       ", SelectConnectionString(context));
            LogHelper.LogSettingMessage(context.Logger, "Frequency      ", context.Frequency?.ToString());
            LogHelper.LogSettingMessage(context.Logger, "Log File       ", context.LogFile?.ToString());
            LogHelper.LogSettingMessage(context.Logger, "Dropbox Folder ", context.DropBoxFolder?.ToString());
            LogHelper.LogSettingMessage(context.Logger, "NflFeedsFile   ", context.NflFeedsFile?.ToString());
            return Some(context);
        }

        private static void WriteSettings(
            ILogger? logger,
            string category,
            string[]? settings)
        {
            if (settings == null)
                return;
            LogHelper.LogSettingMessage(
                logger,
                category,
                string.Join(", ", settings));
        }

        private static string DaysSince(
            FeederReaderContext context) =>

                context.StartDateTime.HasValue
                    ? (DateTime.Now - context.StartDateTime).Value.TotalDays.ToString("F1")
                    : "Missing StartDateTime";


    }


}
