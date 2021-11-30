using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApiDTO.Infrastructure.Loggers;

namespace TodoApiDTO.Infrastructure.Extensions
{
    public static class FileLoggerExtensions
    {
        public static ILoggerFactory AddLogFile(this ILoggerFactory factory, string filePath)
        {
            factory.AddProvider(new FileLoggerProvider(filePath));
            return factory;
        }
    }
}
