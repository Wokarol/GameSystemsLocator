using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wokarol.GameSystemsLocator.Tests
{
    internal class TestLogHandler : ILogHandler
    {
        private readonly List<string> errorsPrinted = new();
        private readonly List<string> warningsPrinted = new();

        public IReadOnlyList<string> Errors => errorsPrinted;
        public IReadOnlyList<string> Warnings => warningsPrinted;

        public void Clear()
        {
            errorsPrinted.Clear();
            warningsPrinted.Clear();
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            errorsPrinted.Add(exception.Message);
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (logType == LogType.Error)
                errorsPrinted.Add(string.Format(format, args));

            if (logType == LogType.Warning)
                warningsPrinted.Add(string.Format(format, args));
        }
    }
}