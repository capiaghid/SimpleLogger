﻿#region

using System;
using System.Collections.Generic;

#endregion

namespace SimpleLogger.Logging.Module
{
    public class ModuleManager
    {
        private readonly IDictionary<string, LoggerModule> _modules;

        public ModuleManager()
        {
            _modules = new Dictionary<string, LoggerModule>();
        }

        public void BeforeLog()
        {
            foreach (LoggerModule loggerModule in _modules.Values)
            {
                loggerModule.BeforeLog();
            }
        }

        public void AfterLog(LogMessage logMessage)
        {
            foreach (LoggerModule loggerModule in _modules.Values)
            {
                loggerModule.AfterLog(logMessage);
            }
        }

        public void ExceptionLog(Exception exception)
        {
            foreach (LoggerModule loggerModule in _modules.Values)
            {
                loggerModule.ExceptionLog(exception);
            }
        }

        public void Install(LoggerModule module)
        {
            if (!_modules.ContainsKey(module.Name))
            {
                module.Initialize();
                _modules.Add(module.Name, module);
            }
            else
            {
                // reinstall module
                Uninstall(module.Name);
                Install(module);
            }
        }

        public void Uninstall(LoggerModule module)
        {
            if (_modules.ContainsKey(module.Name))
            {
                _modules.Remove(module.Name);
            }
        }

        public void Uninstall(string moduleName)
        {
            if (_modules.ContainsKey(moduleName))
            {
                _modules.Remove(moduleName);
            }
        }
    }
}