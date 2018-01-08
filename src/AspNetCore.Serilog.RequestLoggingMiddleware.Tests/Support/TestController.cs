using Microsoft.AspNetCore.Mvc;
using System;

namespace AspNetCore.Serilog.RequestLoggingMiddleware.Tests.Support
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Error(object body) => throw new Exception();
    }
}
