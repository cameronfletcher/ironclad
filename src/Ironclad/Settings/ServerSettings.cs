// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

#pragma warning disable CA1034, CA1056

namespace Ironclad.Settings
{
    using System.Collections.Generic;
    using System.Linq;
    using Ironclad.Extensions;
    using Ironclad.Sdk;

    public sealed class ServerSettings
    {
        [SensitiveData]
        public string Database { get; set; }

        public string IssuerUri { get; set; }

        public bool RespectXForwardedForHeaders { get; set; }

        public SigningCertificateSettings SigningCertificate { get; set; }

        public DataProtectionSettings DataProtection { get; set; }

        public IEnumerable<string> GetValidationErrors(string prefix)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(this.Database))
            {
                errors.Add($"'{prefix}:{nameof(this.Database).ToSnakeCase()}' is null or empty.");
            }

            if (this.SigningCertificate != null)
            {
                errors.AddRange(this.SigningCertificate.GetValidationErrors($"{prefix}:{nameof(this.SigningCertificate).ToSnakeCase()}"));
            }

            if (this.DataProtection != null)
            {
                errors.AddRange(this.DataProtection.GetValidationErrors($"{prefix}:{nameof(this.DataProtection).ToSnakeCase()}"));
            }

            return errors;
        }

        public sealed class SigningCertificateSettings
        {
            public string Filepath { get; set; }

            [SensitiveData]
            public string Password { get; set; }

            public string Thumbprint { get; set; }

            public string CertificateId { get; set; }

            public IEnumerable<string> GetValidationErrors(string prefix)
            {
                var keys = string.Join(
                    ", ",
                    new[] { nameof(this.Thumbprint), nameof(this.Filepath), nameof(this.CertificateId) }.Select(name => $"'{prefix}:{name.ToSnakeCase()}'"));

                if (string.IsNullOrEmpty(this.Filepath) && string.IsNullOrEmpty(this.Thumbprint) && string.IsNullOrEmpty(this.CertificateId))
                {
                    yield return $"All of the following configuration settings are either null or empty (which is invalid): {keys}.";
                }

                if (new[] { string.IsNullOrEmpty(this.Filepath), string.IsNullOrEmpty(this.Thumbprint), string.IsNullOrEmpty(this.CertificateId) }
                    .Where(condition => !condition)
                    .Count() > 1)
                {
                    yield return $"More than one of the following configuration settings have values (which is invalid): {keys}.";
                }
            }
        }

        public sealed class DataProtectionSettings
        {
            public string KeyfileUri { get; set; }

            public string KeyId { get; set; }

            public IEnumerable<string> GetValidationErrors(string prefix)
            {
                if (string.IsNullOrEmpty(this.KeyfileUri) || string.IsNullOrEmpty(this.KeyId))
                {
                    yield return $"One or more of '{prefix}:{nameof(this.KeyfileUri).ToSnakeCase()}' and '{prefix}:{nameof(this.KeyId).ToSnakeCase()}' are null or empty.";
                }
            }
        }
    }
}
