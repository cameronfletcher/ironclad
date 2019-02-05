// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

#pragma warning disable CA1034

namespace Ironclad.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ironclad.Extensions;
    using Ironclad.Sdk;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;

    public sealed class AzureSettings
    {
        public KeyVaultSettings KeyVault { get; set; }

        public IEnumerable<string> GetValidationErrors(string prefix) =>
            this.KeyVault == null ? Array.Empty<string>() : this.KeyVault.GetValidationErrors($"{prefix}:{nameof(this.KeyVault).ToSnakeCase()}");

        public sealed class KeyVaultSettings : IDisposable
        {
            private KeyVaultClient client;

            public string Name { get; set; }

            [SensitiveData]
            public string ConnectionString { get; set; }

            public string Endpoint => $"https://{this.Name}.vault.azure.net";

            public KeyVaultClient Client
            {
                get
                {
                    if (this.client != null)
                    {
                        return this.client;
                    }

                    if (this.GetValidationErrors(null).Any())
                    {
                        return null;
                    }

                    var tokenProvider = new AzureServiceTokenProvider(this.ConnectionString);
                    return this.client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));
                }
            }

            public IEnumerable<string> GetValidationErrors(string prefix)
            {
                if (string.IsNullOrEmpty(this.Name))
                {
                    yield return $"'{prefix}:{nameof(this.Name).ToSnakeCase()}' is null or empty.";
                }
            }

            public void Dispose() => this.client?.Dispose();
        }
    }
}
