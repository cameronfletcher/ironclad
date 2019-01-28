// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Tests.Sdk
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    internal class IroncladProbe : IAsyncLifetime
    {
        private readonly string apiUri;
        private readonly int initialWaitTimeInSeconds;
        private readonly int maxWaitTimeInSeconds;

        public IroncladProbe(string apiUri, int initialWaitTimeInSeconds, int maxWaitTimeInSeconds)
        {
            this.apiUri = apiUri ?? throw new ArgumentNullException(nameof(apiUri));
            this.initialWaitTimeInSeconds = initialWaitTimeInSeconds;
            this.maxWaitTimeInSeconds = maxWaitTimeInSeconds;
        }

        public async Task InitializeAsync() => await this.WaitUntilAvailable(true, default).ConfigureAwait(false);

        public Task DisposeAsync() => Task.CompletedTask;

        [DebuggerStepThrough]
        public async Task<bool> WaitUntilAvailable(bool throwOnFalse, CancellationToken token)
        {
            await Task.Delay((int)TimeSpan.FromSeconds(this.initialWaitTimeInSeconds).TotalMilliseconds, token).ConfigureAwait(false);

            using (var client = new HttpClient())
            {
                var maxWaitTimeFromStart = DateTime.UtcNow.AddSeconds(this.maxWaitTimeInSeconds);

                while (DateTime.UtcNow < maxWaitTimeFromStart && !token.IsCancellationRequested)
                {
                    await Task.Delay(500, token).ConfigureAwait(false);

                    try
                    {
                        using (var response = await client.GetAsync(new Uri(this.apiUri), token).ConfigureAwait(false))
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                return true;
                            }
                        }
                    }
                    catch (HttpRequestException)
                    {
                    }
                }
            }

            if (throwOnFalse)
            {
                throw new TimeoutException("The Ironclad instance did not become available in a timely fashion.");
            }

            return false;
        }
    }
}
