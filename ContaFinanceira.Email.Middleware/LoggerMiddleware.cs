﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContaFinanceira.Middleware
{
    public class LoggerMiddleware
    {
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public LoggerMiddleware(RequestDelegate next,
                                ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory.CreateLogger<LoggerMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string correlationId = null;

            if (httpContext.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out StringValues correlationIds))
            {
                correlationId = correlationIds.FirstOrDefault(k => k.Equals(CorrelationIdHeaderKey));

                _logger.LogInformation($"CorrelationId enviada pela requisição: {correlationId}");
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
                httpContext.Request.Headers.Add(CorrelationIdHeaderKey, correlationId);

                _logger.LogInformation($"CorrelationId gerado: {correlationId}");
            }

            httpContext.Response.OnStarting(() =>
            {
                if (!httpContext.Response.Headers.TryGetValue(CorrelationIdHeaderKey, out correlationIds))
                    httpContext.Response.Headers.Add(CorrelationIdHeaderKey, correlationId);

                return Task.CompletedTask;
            });

            await _next.Invoke(httpContext);
        }
    }
}