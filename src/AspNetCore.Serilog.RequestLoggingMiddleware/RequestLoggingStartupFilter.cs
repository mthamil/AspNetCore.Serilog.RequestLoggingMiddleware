using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace AspNetCore.Serilog.RequestLoggingMiddleware
{
    /// <summary>
    /// An <see cref="IStartupFilter"/> that adds request logging middleware.
    /// </summary>
    class RequestLoggingStartupFilter : IStartupFilter
    {
        private readonly Action<RequestLoggingOptions> _configureOptions;

        public RequestLoggingStartupFilter(Action<RequestLoggingOptions> configureOptions)
        {
            _configureOptions = configureOptions;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => 
            app => next(app.UseSerilogRequestLogging(_configureOptions));
    }
}
