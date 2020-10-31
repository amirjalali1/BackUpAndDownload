using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SwatServer.Middleware
{
    public class SafeListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SafeListMiddleware> _logger;
        private readonly string _safeList;

        public SafeListMiddleware(RequestDelegate next, ILogger<SafeListMiddleware> logger, string safeList)
        {
            _safeList = safeList;
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
                var remoteIp = context.Connection.RemoteIpAddress;
                _logger.LogDebug($"Request from Remote IP address: {remoteIp}");

                string[] ip = _safeList.Split(';');

                var bytes = remoteIp.GetAddressBytes();
                var badIp = true;
                foreach (var address in ip)
                {
                    var testIp = IPAddress.Parse(address);
                    if (testIp.GetAddressBytes().SequenceEqual(bytes))
                    {
                        badIp = false;
                        break;
                    }
                }

                if (badIp)
                {
                    _logger.LogInformation($"Forbidden Request from Remote IP address: {remoteIp}");
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
            await _next.Invoke(context);

        }
    }
}
