// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.
#pragma warning disable CA1056 // Uri properties should not be strings

namespace Ironclad.Console.Persistence
{
    using System;

    public class CommandData
    {
        public string Authority { get; set; }

        public string AccessToken { get; set; }

        public DateTime? AccessTokenExpiration { get; set; }

        public string RefreshToken { get; set; }
    }
}
#pragma warning restore CA1056 // Uri properties should not be strings
