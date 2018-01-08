using Microsoft.AspNetCore.Http;
using System;

namespace AspNetCore.Serilog.RequestLoggingMiddleware
{
    /// <summary>
    /// Options for configuring the request logging middleware.
    /// </summary>
    public class RequestLoggingOptions
    {
        /// <summary>
        /// The function used to transform and select information to log from an <see cref="HttpRequest"/>.
        /// </summary>
        public Func<HttpRequest, object> RequestProjection { get; set; } = req => new RequestInfo(req);
    }
}
