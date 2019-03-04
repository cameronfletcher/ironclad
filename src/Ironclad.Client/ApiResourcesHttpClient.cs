// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

#pragma warning disable CA1054

namespace Ironclad.Client
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An HTTP client for managing API resources for the authorization server.
    /// </summary>
    public sealed class ApiResourcesHttpClient : HttpClientBase, IApiResourcesClient
    {
        private const string ApiPath = "/apiresources";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResourcesHttpClient"/> class.
        /// </summary>
        /// <param name="apiUri">The API URI.</param>
        /// <param name="innerHandler">The inner handler.</param>
        public ApiResourcesHttpClient(string apiUri, HttpMessageHandler innerHandler = null)
            : base(apiUri, innerHandler)
        {
        }

        /// <inheritdoc />
        public Task<ResourceSet<ResourceSummary>> GetApiResourceSummariesAsync(
            string startsWith = default,
            int start = 0,
            int size = 20,
            CancellationToken cancellationToken = default) =>
            this.GetAsync<ResourceSet<ResourceSummary>>(
                this.RelativeUrl($"{ApiPath}?name={WebUtility.UrlEncode(startsWith)}&skip={NotNegative(start, nameof(start))}&take={NotNegative(size, nameof(size))}"),
                cancellationToken);

        /// <inheritdoc />
        public Task<ApiResource> GetApiResourceAsync(string resourceName, CancellationToken cancellationToken = default) =>
            this.GetAsync<ApiResource>(this.RelativeUrl($"{ApiPath}/{WebUtility.UrlEncode(NotNull(resourceName, nameof(resourceName)))}"), cancellationToken);

        /// <inheritdoc />
        public Task AddApiResourceAsync(ApiResource resource, CancellationToken cancellationToken = default) =>
            this.SendAsync(HttpMethod.Post, this.RelativeUrl(ApiPath), resource, cancellationToken);

        /// <inheritdoc />
        public Task RemoveApiResourceAsync(string resourceName, CancellationToken cancellationToken = default) =>
            this.DeleteAsync(this.RelativeUrl($"{ApiPath}/{WebUtility.UrlEncode(NotNull(resourceName, nameof(resourceName)))}"), cancellationToken);

        /// <inheritdoc />
        public Task ModifyApiResourceAsync(ApiResource resource, CancellationToken cancellationToken = default) =>
            this.SendAsync(
                HttpMethod.Put,
                this.RelativeUrl($"{ApiPath}/{WebUtility.UrlEncode(NotNull(resource?.Name, "resource.Name"))}"),
                resource,
                cancellationToken);
    }
 }
