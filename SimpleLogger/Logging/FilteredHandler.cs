using System;

namespace SimpleLogger.Logging
{
    internal class FilteredHandler : ILoggerHandler
    {
        public Predicate<LogMessage> Filter { get; set; }

        public ILoggerHandler Handler { get; set; }

        public void Publish(LogMessage logMessage)
        {
            if (Filter(logMessage))
            {
                Handler.Publish(logMessage);
            }
        }
    }
}