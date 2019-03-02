// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Tests.Sdk
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Npgsql;

    internal class PostgresContainer : Container
    {
        private const string ConnectionString = "Host={0};Database=ironclad;Username=postgres;Password=postgres;Port={1}";

        private readonly int port = PortManager.GetNextPort();
        private readonly PostgresProbe probe;

        public PostgresContainer(string dockerTag)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(string.Format(CultureInfo.InvariantCulture, ConnectionString, "localhost", 5432));

            this.Configuration = new ContainerConfiguration
            {
                Image = "postgres",
                Tag = dockerTag,
                ContainerName = "ironclad-integration-postgres",
                ContainerPortBindings = new[]
                {
                    new ContainerConfiguration.PortBinding
                    {
                        GuestTcpPort = connectionStringBuilder.Port, HostTcpPort = this.port
                    }
                },
                ContainerEnvironmentVariables = new[]
                {
                    "POSTGRES_PASSWORD=" + connectionStringBuilder.Password,
                    "POSTGRES_DB=" + connectionStringBuilder.Database
                },
                OutputDockerLogs = true
            };

            this.probe = new PostgresProbe(this.GetConnectionStringForHost(), 4, 20);
        }

        public string GetConnectionStringForHost() => string.Format(CultureInfo.InvariantCulture, ConnectionString, "localhost", this.port);

        public string GetConnectionStringForContainer() => string.Format(CultureInfo.InvariantCulture, ConnectionString, ResolveHost(), this.port);

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            await this.probe.WaitUntilAvailable(true, default).ConfigureAwait(false);
        }

        private static string ResolveHost() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Environment.MachineName : "host.docker.internal";
    }
}