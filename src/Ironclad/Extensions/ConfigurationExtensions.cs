// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Extensions
{
    using System;
    using Ironclad.Settings;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;

    internal static class ConfigurationExtensions
    {
        internal static IConfigurationBuilder AddAzureKeyVaultFromConfig(this IConfigurationBuilder builder, string[] args)
        {
            const string key = "azure:key_vault";

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Startup>()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var settings = configuration.GetSection(key)?.Get<AzureSettings.KeyVaultSettings>(options => options.BindPropertiesUsingSnakeCaseNamingStrategy = true);
            if (settings == null)
            {
                return builder;
            }

            var errors = settings.GetValidationErrors(key);
            IroncladSettings.ThrowIfConfigurationErrorsHack(errors);

            return builder.AddAzureKeyVault(settings.Endpoint, settings.Client, new UnderscoreKeyVaultSecretManager());
        }

        private class UnderscoreKeyVaultSecretManager : IKeyVaultSecretManager
        {
            public bool Load(SecretItem secret) => true;

            public string GetKey(SecretBundle secret)
            {
                // Replace one dash in any name with an underscore and replace two
                // dashes in any name with the KeyDelimiter, which is the
                // delimiter used in configuration (usually a colon). Azure
                // Key Vault doesn't allow a colon in secret names or an underscore.
                return secret.SecretIdentifier.Name
                    .Replace("--", ConfigurationPath.KeyDelimiter, StringComparison.OrdinalIgnoreCase)
                    .Replace("-", "_", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
