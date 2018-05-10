using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DNetCMS.Middleware
{
    public class ExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;
        private readonly RequestDelegate _next;

        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            try
            {
                await _next(httpContext);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Cathed exception");
            }
        }
    }
}
