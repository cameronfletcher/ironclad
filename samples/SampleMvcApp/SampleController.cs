namespace SampleMvcApp
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;

    public class SampleController : Controller
    {
        public IActionResult Index() => this.View();

        [Authorize]
        public IActionResult Secure() => this.View();

        [Authorize]
        public async Task<IActionResult> CallApi()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.SetBearerToken(token);

            var response = await client.GetStringAsync("http://localhost:5007/api");
            ViewBag.Json = response;

            return View();
        }

        public async Task<IActionResult> RenewTokens()
        {
            var disco = await DiscoveryClient.GetAsync("http://localhost:5005");
            if (disco.IsError) throw new Exception(disco.Error);

            var tokenClient = new TokenClient(disco.TokenEndpoint, "sample_mvc", "secret");
            var rt = await HttpContext.GetTokenAsync("refresh_token");
            var tokenResult = await tokenClient.RequestRefreshTokenAsync(rt);

            if (!tokenResult.IsError)
            {
                var old_id_token = await HttpContext.GetTokenAsync("id_token");
                var new_access_token = tokenResult.AccessToken;
                var new_refresh_token = tokenResult.RefreshToken;

                var tokens = new List<AuthenticationToken>();
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.IdToken, Value = old_id_token });
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.AccessToken, Value = new_access_token });
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.RefreshToken, Value = new_refresh_token });

                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                tokens.Add(new AuthenticationToken { Name = "expires_at", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) });

                var info = await HttpContext.AuthenticateAsync("Cookies");
                info.Properties.StoreTokens(tokens);
                await HttpContext.SignInAsync("Cookies", info.Principal, info.Properties);

                return Redirect("~/Secure");
            }

            ViewData["Error"] = tokenResult.Error;
            return View("Error");
        }

        public IActionResult Logout() => new SignOutResult(new[] { "Cookies", "oidc" });

        public IActionResult Error() => View();
    }
}