namespace SampleTests
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Ironclad.Client;
    using Ironclad.Tests.Sdk;

    // NOTE (Cameron): Here we override the initialization method to set up the auth server for all tests in the collection.
    public class SampleAuthenticationFixture : AuthenticationFixture
    {
        public Client Client { get; } = new Client
        {
            Id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
            Name = $"SampleTests (integration test)",
            AllowedCorsOrigins = { "http://localhost:5006" },
            RedirectUris = { "http://localhost:5006/redirect" },
            AllowedScopes = { "openid", "profile", "sample_api" },
            AllowAccessTokensViaBrowser = true,
            AllowedGrantTypes = { "implicit" },
            RequireConsent = false,
        };

        protected override async Task OnInitializeAsync()
        {
            await base.OnInitializeAsync();

            await this.ClientsClient.AddClientAsync(this.Client);
        }
    }
}
