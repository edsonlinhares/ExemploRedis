using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ExemploRedis.Filters
{
    public class ApiActionFilter : IActionFilter
    {
        private Stopwatch sw = new Stopwatch();
        private readonly ILogger<ApiActionFilter> _logger;
        private IDictionary<string, object> _ActionArguments;

        public ApiActionFilter(ILogger<ApiActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            sw.Start();

            _ActionArguments = context.ActionArguments;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            TimeSpan ts = sw.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            var result = context.Result as ObjectResult;

            _logger.LogInformation(
                "{Request} | {Result} | {Tempo}",
                JsonConvert.SerializeObject(_ActionArguments),
                JsonConvert.SerializeObject(result),
                elapsedTime);
        }
       
    }

}
