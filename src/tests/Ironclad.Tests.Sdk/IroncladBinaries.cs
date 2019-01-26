// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Tests.Sdk
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using Xunit;

    internal class IroncladBinaries : IAsyncLifetime
    {
        private readonly string authority;
        private readonly string apiUri;
        private readonly int port;
        private readonly string connectionString;
        private readonly IroncladProbe probe;

        private Process process;

        public IroncladBinaries(string authority, string apiUri, int port, string connectionString)
        {
            this.authority = authority ?? throw new ArgumentNullException(nameof(authority));
            this.apiUri = authority ?? throw new ArgumentNullException(nameof(apiUri));
            this.port = port;
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.probe = new IroncladProbe(this.authority, 2, 20);
        }

        public async Task InitializeAsync()
        {
            var path = string.Format(CultureInfo.InvariantCulture, "..{0}..{0}..{0}..{0}..{0}Ironclad{0}Ironclad.csproj", Path.DirectorySeparatorChar);
            var arguments = $"run -p {path} -- --server:database=\"{this.connectionString}\" --urls=http://*:{this.port} --api:authority={this.authority} --api:uri={this.apiUri} --api:secret=secret";

            this.process = Process.Start(
                new ProcessStartInfo("dotnet", arguments)
                {
                    UseShellExecute = !Environment.OSVersion.Platform.Equals(PlatformID.Unix)
                });

            await this.probe.WaitUntilAvailable(true, default).ConfigureAwait(false);
        }

        public Task DisposeAsync()
        {
            if (this.process == null)
            {
                return Task.CompletedTask;
            }

            try
            {
                if (Environment.OSVersion.Platform.Equals(PlatformID.Unix))
                {
                    using (var killer = Process.Start(new ProcessStartInfo("pkill", $"-TERM -P {this.process.Id}")))
                    {
                        killer?.WaitForExit();
                    }
                }
                else
                {
                    using (var killer = Process.Start(new ProcessStartInfo("taskkill", $"/T /F /PID {this.process.Id}")))
                    {
                        killer?.WaitForExit();
                    }
                }
            }
            catch (Win32Exception)
            {
            }

            this.process.Dispose();

            return Task.CompletedTask;
        }
    }
}
