#region

using System;

using SimpleLogger.Logging.Formatters;

#endregion

namespace SimpleLogger.Logging
{
    public class LogMessage
    {
        public LogMessage()
        {
        }

        public LogMessage(Logger.Level level, string text, DateTime dateTime, string callingClass, string callingMethod, int lineNumber)
        {
            Level         = level;
            Text          = text;
            DateTime      = dateTime;
            CallingClass  = callingClass;
            CallingMethod = callingMethod;
            LineNumber    = lineNumber;
        }

        public DateTime DateTime { get; set; }

        public Logger.Level Level { get; set; }

        public string Text { get; set; }

        public string CallingClass { get; set; }

        public string CallingMethod { get; set; }

        public int LineNumber { get; set; }

        public override string ToString()
        {
            return new DefaultLoggerFormatter().ApplyFormat(this);
        }
    }
}