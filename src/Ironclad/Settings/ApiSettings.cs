// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

#pragma warning disable CA1056

namespace Ironclad.Settings
{
    using System.Collections.Generic;
    using Ironclad.Extensions;
    using Ironclad.Sdk;

    public sealed class ApiSettings
    {
        public string Authority { get; set; }

        public string Audience { get; set; }

        public string ClientId { get; set; }

        public string OmitUriForRequestsFrom { get; set; }

        [SensitiveData]
        public string Secret { get; set; }

        public string Uri { get; set; }

        public IEnumerable<string> GetValidationErrors(string prefix)
        {
            if (string.IsNullOrEmpty(this.Authority))
            {
                yield return $"'{prefix}:{nameof(this.Authority).ToSnakeCase()}' is null or empty.";
            }

            if (string.IsNullOrEmpty(this.Audience))
            {
                yield return $"'{prefix}:{nameof(this.Audience).ToSnakeCase()}' is null or empty.";
            }

            if (string.IsNullOrEmpty(this.ClientId))
            {
                yield return $"'{prefix}:{nameof(this.ClientId).ToSnakeCase()}' is null or empty.";
            }

            if (string.IsNullOrEmpty(this.Secret))
            {
                yield return $"'{prefix}:{nameof(this.Secret).ToSnakeCase()}' is null or empty.";
            }
        }
    }
}
