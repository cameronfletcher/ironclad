// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Settings
{
    using System.Collections.Generic;
    using Ironclad.Extensions;
    using Ironclad.Sdk;

    public sealed class MailSettings
    {
        public string Sender { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public bool EnableSsl { get; set; }

        public string Username { get; set; }

        [SensitiveData]
        public string Password { get; set; }

        public IEnumerable<string> GetValidationErrors(string prefix)
        {
            if (string.IsNullOrEmpty(this.Sender))
            {
                yield return $"'{prefix}:{nameof(this.Sender).ToSnakeCase()}' is null or empty.";
            }

            if (string.IsNullOrEmpty(this.Host))
            {
                yield return $"'{prefix}:{nameof(this.Host).ToSnakeCase()}' is null or empty.";
            }

            if (!string.IsNullOrEmpty(this.Username) && string.IsNullOrEmpty(this.Password))
            {
                yield return $"'{prefix}:{nameof(this.Password).ToSnakeCase()}' is null or empty but '{prefix}{nameof(this.Username)}' is not.";
            }
        }
    }
}
