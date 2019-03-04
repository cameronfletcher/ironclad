// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

#pragma warning disable CA1054

namespace Ironclad.Client
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents the base class for the HTTP clients.
    /// </summary>
    public class HttpClientBase : IDisposable
    {
        /// <summary>
        /// The default json serializer settings.
        /// </summary>
        protected static readonly JsonSerializerSettings JsonSerializerSettings = GetJsonSerializerSettings();

        private readonly string apiUri;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientBase"/> class.
        /// </summary>
        /// <param name="apiUri">The API URI.</param>
        /// <param name="innerHandler">The inner handler.</param>
        public HttpClientBase(string apiUri, HttpMessageHandler innerHandler = null)
        {
            // TODO (Cameron): Make sure we're working with application/json.
            var handler = innerHandler ?? new HttpClientHandler();

            this.Client = new HttpClient(handler, innerHandler == null);
            this.apiUri = apiUri;
        }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <value>The HTTP client.</value>
        protected HttpClient Client { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>The parameter value.</returns>
        protected static string NotNull(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null.", parameterName);
            }

            return value;
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>The parameter value.</returns>
        protected static int NotNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Value cannot be less than zero.");
            }

            return value;
        }

        /// <summary>
        /// Performs an asynchronous HTTP GET operation.
        /// </summary>
        /// <typeparam name="T">The type of data transfer object.</typeparam>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The data transfer object.</returns>
        protected async Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
        {
            this.EnsureNotDisposed();

            var content = default(string);
            try
            {
                using (var response = await this.Client.GetAsync(requestUri, cancellationToken).EnsureSuccess().ConfigureAwait(false))
                {
                    content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpException(HttpMethod.Get, new Uri(requestUri), ex);
            }

            return JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings);
        }

        /// <summary>
        /// Performs an asynchronous HTTP operation.
        /// </summary>
        /// <typeparam name="T">The type of data transfer object.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task object representing the asynchronous operation.</returns>
        protected async Task SendAsync<T>(HttpMethod method, string requestUri, T resource, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var content = new StringContent(JsonConvert.SerializeObject(resource, JsonSerializerSettings), Encoding.UTF8, "application/json"))
                using (var request = new HttpRequestMessage(method, requestUri) { Content = content })
                using (var response = await this.Client.SendAsync(request, cancellationToken).EnsureSuccess().ConfigureAwait(false))
                {
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpException(method, new Uri(requestUri), ex);
            }
        }

        /// <summary>
        /// Performs an asynchronous HTTP DELETE operation.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task object representing the asynchronous operation.</returns>
        protected async Task DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var response = await this.Client.DeleteAsync(requestUri, cancellationToken).EnsureSuccess().ConfigureAwait(false))
                {
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpException(HttpMethod.Delete, new Uri(requestUri), ex);
            }
        }

        /// <summary>
        /// Returns a URL relative to the authority.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A URL.</returns>
#pragma warning disable CA1055
        protected string RelativeUrl(string path) => this.apiUri + path;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">Set to <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                this.disposed = true;
                this.Client.Dispose();
            }
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                NullValueHandling = NullValueHandling.Ignore,
            };

            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }

        private void EnsureNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}
