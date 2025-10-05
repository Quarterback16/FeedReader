using Microsoft.Extensions.Logging;

namespace FeedReader
{
    /// <summary>
    ///  A custom config object
    /// </summary>
    public class FeederReaderContext
    {
        public ILogger? Logger { get; set; }
        public StreamWriter? StreamWriter { get; set; }
        public ConnectionStrings? ConnectionStrings { get; set; }
        public string[]? AppNames { get; set; }
        public string[]? Exceptions { get; set; }
        public string? ConnectTo { get; set; }
        public DateTime? StartDateTime { get; set; }
        public int? Frequency { get; set; }
        public int? GoBackHours { get; set; }

        public bool? Beep { get; set; }
        public bool? Stats { get; set; }
        public string? KnockOffTime { get; set; }
        public string? LogFile { get; set; }
        public string? DropBoxFolder { get; set; }
        public string? StopWordsFile { get; set; }
        public string? GoWordsFile { get; set; }
        public string? NflFeedsFile { get; set; }

        public bool? MyRoster { get; set; }
        public bool? AllNews { get; set; }

        public string? Season { get; set; }
    }

    public class ConnectionStrings
    {
        public string? Dev { get; set; }
        public string? Prod { get; set; }
    }
}
