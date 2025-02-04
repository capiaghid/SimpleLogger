﻿#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using SimpleLogger.Logging;
using SimpleLogger.Logging.Handlers;
using SimpleLogger.Logging.Module;

#endregion

namespace SimpleLogger
{
    public static class Logger
    {
        public enum Level
        {
            None,
            Debug,
            Fine,
            Info,
            Warning,
            Error,
            Severe
        }

        private readonly static LogPublisher LogPublisher;

        private readonly static object Sync           = new object();
        private static          bool   _isTurned      = true;
        private static          bool   _isTurnedDebug = true;

        static Logger()
        {
            lock (Sync)
            {
                LogPublisher = new LogPublisher();
                Modules      = new ModuleManager();
                Debug        = new DebugLogger();
            }
        }

        public static Level DefaultLevel { get; set; } = Level.Info;

        public static ILoggerHandlerManager LoggerHandlerManager => LogPublisher;

        public static IEnumerable<LogMessage> Messages => LogPublisher.Messages;

        public static DebugLogger Debug { get; }

        public static ModuleManager Modules { get; }

        public static bool StoreLogMessages
        {
            get => LogPublisher.StoreLogMessages;
            set => LogPublisher.StoreLogMessages = value;
        }

        public static void DefaultInitialization()
        {
            LoggerHandlerManager
               .AddHandler(new ConsoleLoggerHandler())
               .AddHandler(new FileLoggerHandler());

            Log(Level.Info, "Default initialization");
        }

        public static void Log()
        {
            Log("There is no message");
        }

        public static void Log(string message)
        {
            Log(DefaultLevel, message);
        }

        public static void Log(Level level, string message)
        {
            StackFrame stackFrame = FindStackFrame();
            MethodBase methodBase = GetCallingMethodBase(stackFrame);
            string callingMethod = methodBase.Name;
            string callingClass = methodBase.ReflectedType.Name;
            int lineNumber = stackFrame.GetFileLineNumber();

            Log(level, message, callingClass, callingMethod, lineNumber);
        }

        public static void Log(Exception exception)
        {
            Log(Level.Error, exception.Message);
            Modules.ExceptionLog(exception);
        }

        public static void Log<TClass>(Exception exception) where TClass : class
        {
            string message = string.Format("Log exception -> Message: {0}\nStackTrace: {1}", exception.Message,
                exception.StackTrace);
            Log<TClass>(Level.Error, message);
        }

        public static void Log<TClass>(string message) where TClass : class
        {
            Log<TClass>(DefaultLevel, message);
        }

        public static void Log<TClass>(Level level, string message) where TClass : class
        {
            StackFrame stackFrame = FindStackFrame();
            MethodBase methodBase = GetCallingMethodBase(stackFrame);
            string callingMethod = methodBase.Name;
            string callingClass = typeof(TClass).Name;
            int lineNumber = stackFrame.GetFileLineNumber();

            Log(level, message, callingClass, callingMethod, lineNumber);
        }

        private static void Log(Level level, string message, string callingClass, string callingMethod, int lineNumber)
        {
            if (!_isTurned || !_isTurnedDebug && level == Level.Debug)
            {
                return;
            }

            DateTime currentDateTime = DateTime.Now;

            Modules.BeforeLog();
            LogMessage logMessage = new LogMessage(level, message, currentDateTime, callingClass, callingMethod, lineNumber);
            LogPublisher.Publish(logMessage);
            Modules.AfterLog(logMessage);
        }

        private static MethodBase GetCallingMethodBase(StackFrame stackFrame)
        {
            return stackFrame == null
                ? MethodBase.GetCurrentMethod()
                : stackFrame.GetMethod();
        }

        private static StackFrame FindStackFrame()
        {
            StackTrace stackTrace = new StackTrace();

            for (int i = 0; i < stackTrace.GetFrames().Count(); i++)
            {
                MethodBase methodBase = stackTrace.GetFrame(i).GetMethod();
                string name = MethodBase.GetCurrentMethod().Name;

                if (!methodBase.Name.Equals("Log") && !methodBase.Name.Equals(name))
                {
                    return new StackFrame(i, true);
                }
            }

            return null;
        }

        public static void On()
        {
            _isTurned = true;
        }

        public static void Off()
        {
            _isTurned = false;
        }

        public static void DebugOn()
        {
            _isTurnedDebug = true;
        }

        public static void DebugOff()
        {
            _isTurnedDebug = false;
        }

        private static class FilterPredicates
        {
            public static bool ByLevelHigher(Level logMessLevel, Level filterLevel)
            {
                return (int)logMessLevel >= (int)filterLevel;
            }

            public static bool ByLevelLower(Level logMessLevel, Level filterLevel)
            {
                return (int)logMessLevel <= (int)filterLevel;
            }

            public static bool ByLevelExactly(Level logMessLevel, Level filterLevel)
            {
                return (int)logMessLevel == (int)filterLevel;
            }

            public static bool ByLevel(LogMessage logMessage, Level filterLevel, Func<Level, Level, bool> filterPred)
            {
                return filterPred(logMessage.Level, filterLevel);
            }
        }

        public class FilterByLevel
        {
            public FilterByLevel(Level level)
            {
                FilteredLevel   = level;
                ExactlyLevel    = true;
                OnlyHigherLevel = true;
            }

            public FilterByLevel()
            {
                ExactlyLevel    = false;
                OnlyHigherLevel = true;
            }

            public Level FilteredLevel { get; set; }

            public bool ExactlyLevel { get; set; }

            public bool OnlyHigherLevel { get; set; }

            public Predicate<LogMessage> Filter
            {
                get
                {
                    return delegate(LogMessage logMessage)
                    {
                        return FilterPredicates.ByLevel(logMessage, FilteredLevel, delegate(Level lm, Level fl)
                        {
                            return ExactlyLevel ? FilterPredicates.ByLevelExactly(lm, fl) :
                                OnlyHigherLevel ? FilterPredicates.ByLevelHigher(lm, fl) :
                                FilterPredicates.ByLevelLower(lm, fl);
                        });
                    };
                }
            }
        }
    }
}