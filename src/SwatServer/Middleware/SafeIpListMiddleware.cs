﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SwatServer.Middleware
{
    public class SafeIpListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SafeIpListMiddleware> _logger;
        private readonly string _safeIpList;

        public SafeIpListMiddleware(RequestDelegate next, ILogger<SafeIpListMiddleware> logger, string safeIpList)
        {
            _safeIpList = safeIpList;
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
                var remoteIp = context.Connection.RemoteIpAddress;
                _logger.LogDebug($"Request from Remote IP address: {remoteIp}");

                string[] ipList = _safeIpList.Split(';');

                var bytes = remoteIp.GetAddressBytes();
                var badIp = true;
                foreach (var ip in ipList)
                {
                    var testIp = IPAddress.Parse(ip);
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
