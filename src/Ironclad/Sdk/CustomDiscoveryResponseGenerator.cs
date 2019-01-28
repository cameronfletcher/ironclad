// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

#pragma warning disable CA1054 // not required for this class

namespace Ironclad.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdentityServer4.Configuration;
    using IdentityServer4.ResponseHandling;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using IdentityServer4.Validation;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class CustomDiscoveryResponseGenerator : DiscoveryResponseGenerator
    {
        private readonly string omitApiUriForRequestsFrom;

        // NOTE (Cameron): This is a bit of a hack - passing in the IServiceProvider then resolving the services - but we can live with it because it's internal.
        public CustomDiscoveryResponseGenerator(IServiceProvider serviceProvider, string omitApiUriForRequestsFrom)
            : base(
                serviceProvider.GetRequiredService<IdentityServerOptions>(),
                serviceProvider.GetRequiredService<IResourceStore>(),
                serviceProvider.GetRequiredService<IKeyMaterialService>(),
                serviceProvider.GetRequiredService<ExtensionGrantValidator>(),
                serviceProvider.GetRequiredService<SecretParser>(),
                serviceProvider.GetRequiredService<IResourceOwnerPasswordValidator>(),
                serviceProvider.GetRequiredService<ILogger<DiscoveryResponseGenerator>>())
        {
            this.omitApiUriForRequestsFrom = omitApiUriForRequestsFrom;
        }

        public override async Task<Dictionary<string, object>> CreateDiscoveryDocumentAsync(string baseUrl, string issuerUri)
        {
            var dictionary = await base.CreateDiscoveryDocumentAsync(baseUrl, issuerUri);
            if (!dictionary.TryGetValue("api_uri", out var apiUri))
            {
                return dictionary;
            }

            if (string.Equals(baseUrl, this.omitApiUriForRequestsFrom, StringComparison.OrdinalIgnoreCase))
            {
                apiUri = null;
            }

            // HACK (Cameron): This is nonsense. Dictionary<,> doesn't guarantee order. But it works on my machine. :-)
            var entries = new Dictionary<string, object>();
            foreach (var entry in dictionary)
            {
                if (string.Equals(entry.Key, "api_uri", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                entries.Add(entry.Key, entry.Value);

                if (apiUri != null && string.Equals(entry.Key, "issuer", StringComparison.OrdinalIgnoreCase))
                {
                    entries.Add("api_uri", apiUri);
                }
            }

            return entries;
        }
    }
}
