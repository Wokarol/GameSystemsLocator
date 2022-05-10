using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wokarol.GameSystemsLocator.Tests
{
    internal class TestLogHandler : ILogHandler
    {
        private readonly List<string> errorsPrinted = new();

        public IReadOnlyList<string> Errors => errorsPrinted;

        public void Clear() => errorsPrinted.Clear();

        public void LogException(Exception exception, UnityEngine.Object context)
        {

        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (logType == LogType.Error)
                errorsPrinted.Add(string.Format(format, args));
        }
    }
}