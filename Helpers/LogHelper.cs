using Microsoft.Extensions.Logging;

namespace FeedReader.Helpers
{
    public static partial class LogHelper
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "{anyMessage}")]
        public static partial void ErrorMessage(
            ILogger? logger,
            string? anyMessage);

        [LoggerMessage(Level = LogLevel.Information, Message = "{anyMessage}")]
        public static partial void LogMessage(
            ILogger? logger,
            string? anyMessage);

        [LoggerMessage(Level = LogLevel.Information, Message = "Butler Core {startDate}.")]
        public static partial void LogStartupMessage(
            ILogger? logger,
            string? startDate);

        [LoggerMessage(Level = LogLevel.Information, Message = "{settingName}: {settingValue}.")]
        public static partial void LogSettingMessage(
            ILogger? logger,
            string settingName,
            string? settingValue);

        [LoggerMessage(Level = LogLevel.Information, Message = "{line}")]
        public static partial void LogErrorLine(
            ILogger? logger,
            string line);
    }
}
