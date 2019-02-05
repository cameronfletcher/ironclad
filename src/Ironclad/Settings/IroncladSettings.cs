// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ironclad.Extensions;

    // TODO (Cameron): This class is a huge mess - for many reasons. Something needs to be done...
    public sealed class IroncladSettings
    {
        public ServerSettings Server { get; set; }

        public ApiSettings Api { get; set; }

        public IdpSettings Idp { get; set; }

        public MailSettings Mail { get; set; }

        public AzureSettings Azure { get; set; }

        public IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (this.Server == null)
            {
                errors.Add($"'{nameof(this.Server).ToSnakeCase()}' section missing");
            }

            errors.AddRange(this.Server?.GetValidationErrors(nameof(this.Server).ToSnakeCase()) ?? Array.Empty<string>());
            errors.AddRange(this.Api?.GetValidationErrors(nameof(this.Api).ToSnakeCase()) ?? Array.Empty<string>());
            errors.AddRange(this.Idp?.GetValidationErrors(nameof(this.Idp).ToSnakeCase()) ?? Array.Empty<string>());
            errors.AddRange(this.Mail?.GetValidationErrors(nameof(this.Mail).ToSnakeCase()) ?? Array.Empty<string>());
            errors.AddRange(this.Azure?.GetValidationErrors(nameof(this.Azure).ToSnakeCase()) ?? Array.Empty<string>());

            return errors;
        }

        public void Validate() => ThrowIfConfigurationErrorsHack(this.GetValidationErrors());

        internal static void ThrowIfConfigurationErrorsHack(IEnumerable<string> errors)
        {
            if (errors.Any())
            {
                // TODO (Cameron): Change link to point to somewhere sensible (when it exists).
                throw new InvalidOperationException(
                    $@"Validation of configuration settings failed.\r\n{string.Join("\r\n", errors)}
Please see https://gist.github.com/cameronfletcher/58673a468c8ebbbf91b81e706063ba56 for more information.");
            }
        }
    }
}
