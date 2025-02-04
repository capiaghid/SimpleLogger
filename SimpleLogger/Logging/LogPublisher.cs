﻿#region

using System;
using System.Collections.Generic;

#endregion

namespace SimpleLogger.Logging
{
    internal class LogPublisher : ILoggerHandlerManager
    {
        private readonly IList<ILoggerHandler> _loggerHandlers;
        private readonly IList<LogMessage>     _messages;

        public LogPublisher()
        {
            _loggerHandlers  = new List<ILoggerHandler>();
            _messages        = new List<LogMessage>();
            StoreLogMessages = false;
        }

        public LogPublisher(bool storeLogMessages)
        {
            _loggerHandlers  = new List<ILoggerHandler>();
            _messages        = new List<LogMessage>();
            StoreLogMessages = storeLogMessages;
        }

        public IEnumerable<LogMessage> Messages => _messages;

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="SimpleLogger.Logging.LogPublisher" /> store log messages.
        /// </summary>
        /// <value><c>true</c> if store log messages; otherwise, <c>false</c>. By default is <c>false</c></value>
        public bool StoreLogMessages { get; set; }

        public ILoggerHandlerManager AddHandler(ILoggerHandler loggerHandler)
        {
            if (loggerHandler != null)
            {
                _loggerHandlers.Add(loggerHandler);
            }

            return this;
        }

        public ILoggerHandlerManager AddHandler(ILoggerHandler loggerHandler, Predicate<LogMessage> filter)
        {
            if (filter == null || loggerHandler == null)
            {
                return this;
            }

            return AddHandler(new FilteredHandler
            {
                Filter  = filter,
                Handler = loggerHandler
            });
        }

        public bool RemoveHandler(ILoggerHandler loggerHandler)
        {
            return _loggerHandlers.Remove(loggerHandler);
        }

        public void Publish(LogMessage logMessage)
        {
            if (StoreLogMessages)
            {
                _messages.Add(logMessage);
            }

            foreach (ILoggerHandler loggerHandler in _loggerHandlers)
            {
                loggerHandler.Publish(logMessage);
            }
        }
    }
}