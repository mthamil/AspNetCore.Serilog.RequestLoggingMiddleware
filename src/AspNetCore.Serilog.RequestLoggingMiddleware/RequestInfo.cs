using Microsoft.AspNetCore.Http;

namespace AspNetCore.Serilog.RequestLoggingMiddleware
{
    /// <summary>
    /// The default representation of request information.
    /// </summary>
    class RequestInfo
    {
        public RequestInfo(HttpRequest request)
        {
            ContentLength = request.ContentLength;
            ContentType = request.ContentType;
            Protocol = request.Protocol;
            Host = request.Host.Value;
            IsAuthenticated = request.HttpContext.User?.Identity?.IsAuthenticated ?? false;
        }

        public long? ContentLength { get; }
        public string ContentType { get; }
        public string Protocol { get; }
        public string Host { get; }
        public bool IsAuthenticated { get; }
    }
}
