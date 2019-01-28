// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Sdk
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class AuthCookieMiddleware
    {
        private static Regex regex = new Regex(@"OS ((\d+_?){2,3})\s", RegexOptions.Compiled);
        private readonly RequestDelegate next;

        public AuthCookieMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!RequiresSameSiteCookieFix(context))
            {
                await this.next.Invoke(context);
                return;
            }

            var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var handlerProvider = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();

            foreach (var scheme in await schemeProvider.GetRequestHandlerSchemesAsync())
            {
                if (await handlerProvider.GetHandlerAsync(context, scheme.Name) is IAuthenticationRequestHandler handler &&
                    await handler.HandleRequestAsync())
                {
                    string location = null;
                    if (context.Response.StatusCode == (int)HttpStatusCode.Redirect)
                    {
                        location = context.Response.Headers["location"];
                    }
                    else if (context.Request.Method == "GET" && !context.Request.Query["skip"].Any())
                    {
                        location = context.Request.Path + context.Request.QueryString + "&skip=1";
                    }

                    if (location != null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;

                        var html = $@"
                                <html><head>
                                    <meta http-equiv='refresh' content='0;url={location}' />
                                </head></html>";
                        await context.Response.WriteAsync(html);
                    }

                    return;
                }
            }

            await this.next.Invoke(context);
        }

        // LINK: https://github.com/IdentityServer/IdentityServer4/issues/2595#issuecomment-425068595
        private static bool RequiresSameSiteCookieFix(HttpContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var groups = regex.Matches(userAgent);

            if (groups.Count == 0)
            {
                return false;
            }

            var captures = groups[0].Captures;

            if (captures.Count == 0)
            {
                return false;
            }

            // Captured version might be in a form of a semver, ie. 'OS 10_3_0'
            var version = captures[0].Value
                .Replace("OS ", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                .Split('_', StringSplitOptions.None)[0];

            return int.TryParse(version, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) && v >= 12;
        }
    }
}
