// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

#pragma warning disable CA1034

namespace Ironclad.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ironclad.Extensions;

    public sealed class IdpSettings
    {
        public IEnumerable<string> RestrictedDomains { get; set; }

        public GoogleSettings Google { get; set; }

        public IEnumerable<string> GetValidationErrors(string prefix) =>
            this.Google != null ? this.Google.GetValidationErrors($"{prefix}:{nameof(this.Google).ToSnakeCase()}") : Array.Empty<string>();

        public class GoogleSettings
        {
            public string ClientId { get; set; }

            public IEnumerable<string> GetValidationErrors(string prefix)
            {
                if (string.IsNullOrEmpty(this.ClientId))
                {
                    yield return $"'{prefix}:{nameof(this.ClientId).ToSnakeCase()}' is null or empty.";
                }
            }
        }
    }
}
