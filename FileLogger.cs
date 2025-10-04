﻿using Microsoft.Extensions.Logging;

namespace FeedReader
{
    public class CustomFileLoggerProvider(StreamWriter logFileWriter) : ILoggerProvider
    {
        private readonly StreamWriter _logFileWriter = logFileWriter
            ?? throw new ArgumentNullException(nameof(logFileWriter));

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomFileLogger(
                categoryName,
                _logFileWriter);
        }

        public void Dispose() => _logFileWriter.Dispose();
    }

    // Customized ILogger, writes logs to text files
    public class CustomFileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly StreamWriter _logFileWriter;

        public CustomFileLogger(
            string categoryName,
            StreamWriter logFileWriter)
        {
            _categoryName = categoryName;
            _logFileWriter = logFileWriter;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // required by the ILogger interface
            return null;
        }

        public bool IsEnabled(
            LogLevel logLevel)
        {
            // Ensure that only information level and higher logs are recorded
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            // Ensure that only information level and higher logs are recorded
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // Get the formatted log message
            var message = formatter(state, exception);

            // Write log messages to text file
            _logFileWriter.WriteLine($"[{DateTime.Now:hh:mm:ss}]:{message}");
            _logFileWriter.Flush();
        }
    }
}