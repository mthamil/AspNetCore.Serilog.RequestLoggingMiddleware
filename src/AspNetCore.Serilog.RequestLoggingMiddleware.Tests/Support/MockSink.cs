using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;

namespace AspNetCore.Serilog.RequestLoggingMiddleware.Tests.Support
{
    class MockSink : ILogEventSink
    {
        private readonly List<LogEvent> _log = new List<LogEvent>();

        public IReadOnlyList<LogEvent> Log => _log;

        public void Emit(LogEvent logEvent) => _log.Add(logEvent);
    }
}
