# ASP.NET Core Serilog Request Logging Middleware
Middleware for AspNet Core that uses Serilog to log requests. This project was inspired directly by the following post https://blog.getseq.net/smart-logging-middleware-for-asp-net-core/.

[![Build status](https://ci.appveyor.com/api/projects/status/m6w18r01hk34pa4p/branch/master?svg=true)](https://ci.appveyor.com/project/mthamil/aspnetcore-serilog-requestloggingmiddleware/branch/master)


Download
========
Visit [![NuGet](https://img.shields.io/nuget/v/AspNetCore.Serilog.RequestLoggingMiddleware.svg)](https://www.nuget.org/packages/AspNetCore.Serilog.RequestLoggingMiddleware/) to download.


Usage
=====

To use, when configuring the `IApplicationBuilder`, such as in the `Configure` method of `Startup.cs`, add the following:
```c#
    using AspNetCore.Serilog.RequestLoggingMiddleware;
    ...
    public void Configure(IApplicationBuilder app)
    {
        ...
        app.UseSerilogRequestLogging();
        ...
    }
```

**or**, when configuring the `IWebHostBuilder` in `Program.cs`, add the following:
```c#
    using AspNetCore.Serilog.RequestLoggingMiddleware;
    ...
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseSerilogRequestLogging()
    ...
    
```

#### Options
For both methods, an action that configures an instance of `RequestLoggingOptions` can be provided like so:
```c#
    app.UseSerilogRequestLogging(options => // Configure options here.
```

Currently there is only one option, `RequestProjection`, a delegate that defines a mapping determining which properties 
of an `HttpRequest` should be logged.
By default, the following properties are logged:

- ContentLength
- ContentType
- Protocol 
- Host
- IsAuthenticated

This can be customized using the aforementioned options, such as in the following example which logs only the properties 
`IsHttps` and `QueryString`:
```c#
    app.UseSerilogRequestLogging(options => 
        options.RequestProjection = 
            r => new { r.IsHttps, QueryString = r.QueryString.Value });
```

An anonymous object or a named class instance can be provided specifying the properties to log.
**Note**: the object provided will be logged using Serilog object destructuring.