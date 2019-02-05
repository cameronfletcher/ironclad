// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad
{
    using Ironclad.Services.Certificates;
    using Ironclad.Settings;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddSigningCredentialFromSettings(this IIdentityServerBuilder builder, IroncladSettings settings, ILoggerFactory loggerFactory)
        {
            if (settings.Server.SigningCertificate == null)
            {
                var logger = loggerFactory.CreateLogger(typeof(IdentityServerBuilderExtensions));
                logger.LogWarning("No signing credential certificate defined in configuration settings.");

                builder.AddDeveloperSigningCredential();

                return builder;
            }

            var provider = default(ICertificateProvider);

            if (!string.IsNullOrEmpty(settings.Server.SigningCertificate.Filepath))
            {
                provider = new FileCertificateProvider(
                    settings.Server.SigningCertificate.Filepath,
                    settings.Server.SigningCertificate.Password,
                    loggerFactory.CreateLogger<FileCertificateProvider>());
            }
            else if (!string.IsNullOrEmpty(settings.Server.SigningCertificate.Thumbprint))
            {
                provider = new LocalMachineStoreCertificateProvider(
                    settings.Server.SigningCertificate.Thumbprint,
                    loggerFactory.CreateLogger<LocalMachineStoreCertificateProvider>());
            }
            else if (!string.IsNullOrEmpty(settings.Server.SigningCertificate.CertificateId))
            {
                provider = new AzureKeyVaultCertificateProvider(
                    settings.Azure.KeyVault.Client,
                    settings.Server.SigningCertificate.CertificateId,
                    loggerFactory.CreateLogger<AzureKeyVaultCertificateProvider>());
            }

            var certificate = provider.GetCertificateAsync().Result;

            builder.AddSigningCredential(certificate);

            return builder;
        }
    }
}
