using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Serilog.RequestLoggingMiddleware
{
    /// <summary>
    /// Middleware that logs HTTP request and response details.
    /// </summary>
    public class SerilogRequestMiddleware
    {
        private const string MessageTemplate = "HTTP {Method} to '{Path}' responded with {StatusCode} in {Elapsed:0.0000} ms";

        private readonly RequestDelegate _next;
        private readonly RequestLoggingOptions _options;
        private readonly ILogger _logger;

        public SerilogRequestMiddleware(RequestDelegate next, ILogger logger, RequestLoggingOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger?.ForContext<SerilogRequestMiddleware>() ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var stopwatch = new Stopwatch();
            try
            {
                using (new BenchmarkToken(stopwatch))
                {
                    await _next(httpContext);
                }

                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

                var contextualLogger = _logger.ForContext("Request", _options.RequestSelector(httpContext.Request), destructureObjects: true);
                contextualLogger = (level == LogEventLevel.Error ? PopulateLogContext(contextualLogger, httpContext) : contextualLogger);

                contextualLogger.Write(level, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, statusCode, stopwatch.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                PopulateLogContext(_logger, httpContext)
                    .Error(ex, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, 500, stopwatch.Elapsed.TotalMilliseconds);

                throw;
            }
        }

        private static ILogger PopulateLogContext(ILogger logger, HttpContext httpContext)
        {
            var request = httpContext.Request;

            var result = logger
                .ForContext("RequestHeaders", request.Headers
                                                     .Where(h => h.Key != "Authorization")
                                                     .ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true);

            if (request.HasFormContentType)
                result = result.ForContext("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));

            return result;
        }

        /// <summary>
        /// This class ensures that a stopwatch is stopped when an exception occurs without 
        /// needing multiple <see cref="Stopwatch.Stop"/> calls.
        /// </summary>
        class BenchmarkToken : IDisposable
        {
            private readonly Stopwatch _stopwatch;

            public BenchmarkToken(Stopwatch stopwatch)
            {
                _stopwatch = stopwatch;
                _stopwatch.Start();
            }

            public void Dispose() => _stopwatch.Stop();
        }
    }
}
