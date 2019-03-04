// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Tests.Sdk
{
    /// <summary>
    /// Represents an authorization response.
    /// </summary>
    public class AuthorizationResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is error.
        /// </summary>
        /// <value>Returns  <c>true</c> if this instance is in error; otherwise, <c>false</c>.</value>
        public bool IsError { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>The error.</value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the raw response.
        /// </summary>
        /// <value>The raw response.</value>
        public string Raw { get; set; }
    }
}
