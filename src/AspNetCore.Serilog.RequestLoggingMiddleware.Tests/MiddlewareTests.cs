using AspNetCore.Serilog.RequestLoggingMiddleware.Tests.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Serilog.RequestLoggingMiddleware.Tests
{
    public class MiddlewareTests : IDisposable
    {
        [Fact]
        public async Task Test_NonStartupFilter()
        {
            // Arrange.
            _server = new TestServer(ConfigureWebHost(new WebHostBuilder(), app => app.UseSerilogRequestLogging()));
            _client = _server.CreateClient();

            // Act.
            var response = await _client.GetAsync("/api/test");

            // Assert.
            var logEvent = Assert.Single(_sink.Log);
            AssertLogProperties(logEvent, "GET", 200);

            var request = AssertPropertyExists<StructureValue>(logEvent.Properties, "Request");
            AssertAllPropertiesExist(request.Properties.ToDictionary(p => p.Name, p => p.Value),
                "ContentLength", "ContentType", "Protocol", "Host", "IsAuthenticated");
        }

        [Fact]
        public async Task Test_StartupFilter()
        {
            // Arrange.
            _server = new TestServer(ConfigureWebHost(new WebHostBuilder().UseSerilogRequestLogging()));
            _client = _server.CreateClient();

            // Act.
            var response = await _client.GetAsync("/api/test");

            // Assert.
            var logEvent = Assert.Single(_sink.Log);
            AssertLogProperties(logEvent, "GET", 200);

            var request = AssertPropertyExists<StructureValue>(logEvent.Properties, "Request");
            AssertAllPropertiesExist(request.Properties.ToDictionary(p => p.Name, p => p.Value), 
                "ContentLength", "ContentType", "Protocol", "Host", "IsAuthenticated");
        }

        [Fact]
        public async Task Test_CustomRequestInfo()
        {
            // Arrange.
            _server = new TestServer(ConfigureWebHost(new WebHostBuilder(), 
                app => app.UseSerilogRequestLogging(options => 
                    options.RequestProjection = r => new { r.IsHttps, QueryString = r.QueryString.Value })));
            _client = _server.CreateClient();

            // Act.
            var response = await _client.GetAsync("/api/test");

            // Assert.
            var logEvent = Assert.Single(_sink.Log);
            AssertLogProperties(logEvent, "GET", 200);

            var request = AssertPropertyExists<StructureValue>(logEvent.Properties, "Request");
            AssertAllPropertiesExist(request.Properties.ToDictionary(p => p.Name, p => p.Value), "IsHttps", "QueryString");
        }

        [Fact]
        public async Task Test_ExceptionHandling()
        {
            // Arrange.
            _server = new TestServer(ConfigureWebHost(new WebHostBuilder(),
                app => app.UseSerilogRequestLogging(options =>
                    options.RequestProjection = r => new { r.IsHttps, QueryString = r.QueryString.Value })));
            _client = _server.CreateClient();

            // Act.
            try
            {
                var response = await _client.PostAsJsonAsync("/api/test", new { TestBody = "Testing" });
            }
            catch (Exception) { }

            // Assert.
            var logEvent = Assert.Single(_sink.Log);
            Assert.NotNull(logEvent.Exception);
            AssertLogProperties(logEvent, "POST", 500);
        }


        private static void AssertLogProperties(LogEvent logEvent, string method, int statusCode)
        {
            AssertProperty(logEvent.Properties, "Method", method);
            AssertProperty(logEvent.Properties, "Path", "/api/test");
            AssertProperty(logEvent.Properties, "StatusCode", statusCode);
            AssertProperty(logEvent.Properties, "SourceContext", typeof(SerilogRequestMiddleware).FullName);
            AssertPropertyExists<ScalarValue>(logEvent.Properties, "Elapsed");
        }

        private static void AssertProperty<TExpected>(IReadOnlyDictionary<string, LogEventPropertyValue> properties, string propertyName, TExpected expected)
        {
            Assert.Equal(expected, AssertPropertyExists<ScalarValue>(properties, propertyName).Value);
        }

        private static TPropertyValue AssertPropertyExists<TPropertyValue>(IReadOnlyDictionary<string, LogEventPropertyValue> properties, string propertyName)
            where TPropertyValue : LogEventPropertyValue
        {
            return Assert.IsType<TPropertyValue>(Assert.Single(properties, p => p.Key == propertyName).Value);
        }

        private static void AssertAllPropertiesExist(IReadOnlyDictionary<string, LogEventPropertyValue> properties, params string[] propertyNames)
        {
            Assert.All(propertyNames, name => AssertPropertyExists<ScalarValue>(properties, name));
        }

        public MiddlewareTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.Sink(_sink)
                .CreateLogger();
        }

        private IWebHostBuilder ConfigureWebHost(IWebHostBuilder builder, Func<IApplicationBuilder, IApplicationBuilder> configure = null)
        {
            if (configure == null)
                configure = app => app;

            return builder
                .ConfigureServices(services =>
                    services.AddSingleton(Log.Logger)
                            .AddMvcCore()
                            .AddFormatterMappings()
                            .AddDataAnnotations()
                            .AddJsonFormatters()
                            .AddDataAnnotations())
                .Configure(app => configure(app).UseMvc());
        }

        public void Dispose()
        {
            _client?.Dispose();
            _server?.Dispose();
        }

        private readonly MockSink _sink = new MockSink();
        private TestServer _server;
        private HttpClient _client;
    }
}
