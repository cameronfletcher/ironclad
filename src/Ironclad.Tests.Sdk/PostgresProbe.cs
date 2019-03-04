// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Tests.Sdk
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Npgsql;
    using Xunit;

    internal class PostgresProbe : IAsyncLifetime
    {
        private readonly string connectionString;
        private readonly int initialWaitTimeInSeconds;
        private readonly int maxWaitTimeInSeconds;

        public PostgresProbe(string connectionString, int initialWaitTimeInSeconds, int maxWaitTimeInSeconds)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.initialWaitTimeInSeconds = initialWaitTimeInSeconds;
            this.maxWaitTimeInSeconds = maxWaitTimeInSeconds;
        }

        public async Task InitializeAsync() => await this.WaitUntilAvailable(true, default).ConfigureAwait(false);

        public Task DisposeAsync() => Task.CompletedTask;

        public async Task<bool> WaitUntilAvailable(bool throwOnFalse, CancellationToken token)
        {
            await Task.Delay((int)TimeSpan.FromSeconds(this.initialWaitTimeInSeconds).TotalMilliseconds, token).ConfigureAwait(false);

            using (var connection = new NpgsqlConnection(this.connectionString))
            {
                var maxWaitTimeFromStart = DateTime.UtcNow.AddSeconds(this.maxWaitTimeInSeconds);

                while (DateTime.UtcNow < maxWaitTimeFromStart && !token.IsCancellationRequested)
                {
                    await Task.Delay(500, token).ConfigureAwait(false);

                    try
                    {
                        await connection.OpenAsync(token).ConfigureAwait(false);
                        return true;
                    }
                    catch (Exception exception) when (exception is NpgsqlException || exception is SocketException || exception is EndOfStreamException)
                    {
                    }
                }
            }

            if (throwOnFalse)
            {
                throw new TimeoutException("The Postgres instance did not become available in a timely fashion.");
            }

            return false;
        }
    }
}