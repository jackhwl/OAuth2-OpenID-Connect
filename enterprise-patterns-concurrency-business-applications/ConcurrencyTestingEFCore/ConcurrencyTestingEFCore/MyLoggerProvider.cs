using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConcurrencyTestingEFCore
{
    public class MyLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            if(categoryName == "Microsoft.EntityFrameworkCore.Database.Transaction" 
                || categoryName == "Microsoft.EntityFrameworkCore.Database.Connection" 
                || categoryName == "Microsoft.EntityFrameworkCore.Database.Command" 
                || categoryName == "Microsoft.EntityFrameworkCore.Update")
            {
                return new MyLogger();
            }

            return new NullLogger();

        }

        public void Dispose()
        { }

        private class MyLogger : ILogger
        {
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                    int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                Console.Write("ThreadId: {0} :", threadId);
                Console.WriteLine(formatter(state, exception));
                   //File.AppendAllText(@"C:\EFLogs\log.txt", formatter(state, exception));
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }

        private class NullLogger : ILogger
        {
            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            { }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}
