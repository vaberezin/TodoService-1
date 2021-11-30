using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApiDTO.Infrastructure.Loggers
{
    public class FileLogger : ILogger
    {
        private readonly string filePath;
        private static object _lock = new object();
        public FileLogger(string path)
        {
            filePath = path;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // Write only Information logLevel
            if (LogLevel.Information == logLevel)
            {
                return true;
            }
            return false;
        }

        public void Log<TState>(LogLevel logLevel, 
                                EventId eventId, 
                                TState state, 
                                Exception exception, 
                                Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                lock (_lock)
                {
                    File.AppendAllText(filePath, formatter(state, exception) + Environment.NewLine);
                }
            }
        }
    }
}
