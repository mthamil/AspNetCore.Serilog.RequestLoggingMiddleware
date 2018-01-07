using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AspNetCore.Serilog.RequestLoggingMiddleware
{
    /// <summary>
    /// Provides extension methods for registering the request logging middleware.
    /// </summary>
    public static class SerilogRequestMiddlewareExtensions
    {
        /// <summary>
        /// Adds the request logging middleware to the application pipeline.
        /// </summary>
        /// <param name="builder">An application builder.</param>
        /// <param name="configureOptions">An optional action that can be used to configure the middleware options.</param>
        public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder builder, Action<RequestLoggingOptions> configureOptions = null)
        {
            var options = new RequestLoggingOptions();
            configureOptions?.Invoke(options);
            return builder.UseMiddleware<SerilogRequestMiddleware>(options);
        }

        /// <summary>
        /// Adds the request logging middleware using an <see cref="IStartupFilter"/> so that it can run earlier in the pipeline.
        /// </summary>
        /// <param name="builder">A webhost builder.</param>
        /// <param name="configureOptions">An optional action that can be used to configure the middleware options.</param>
        public static IWebHostBuilder UseSerilogRequestLogging(this IWebHostBuilder builder, Action<RequestLoggingOptions> configureOptions = null)
        {
            return builder.ConfigureServices(services => 
                services.AddSingleton<IStartupFilter>(new RequestLoggingStartupFilter(configureOptions)));
        }
    }
}
