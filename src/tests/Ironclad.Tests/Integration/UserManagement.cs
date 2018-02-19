﻿// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Tests.Feature
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using IdentityModel.Client;
    using IdentityModel.OidcClient;
    using Ironclad.Client;
    using Ironclad.Tests.Sdk;
    using Xunit;

    public class UserManagement : IntegrationTest
    {
        public UserManagement(IroncladFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task CanAddUserMinimum()
        {
            // arrange
            var httpClient = new UsersHttpClient(this.Authority, this.Handler);
            var expectedUser = new User
            {
                Username = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
            };

            // act
            var actualUser = await httpClient.AddUserAsync(expectedUser).ConfigureAwait(false);

            // assert
            actualUser.Should().NotBeNull();
            actualUser.Username.Should().Be(expectedUser.Username);
        }

        [Fact]
        public async Task CanAddUser()
        {
            // arrange
            var httpClient = new UsersHttpClient(this.Authority, this.Handler);
            var expectedUser = new User
            {
                Username = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                Password = "password",
                Email = "bob@bob.com",
                PhoneNumber = "123456789",
                Roles = { "admin" },
            };

            // act
            var actualUser = await httpClient.AddUserAsync(expectedUser).ConfigureAwait(false);

            // assert
            actualUser.Should().NotBeNull();
            actualUser.Should().BeEquivalentTo(expectedUser, options => options.Excluding(user => user.Id).Excluding(user => user.Password));
        }

        [Fact]
        public async Task CanGetUserSummaries()
        {
            // arrange
            var httpClient = new UsersHttpClient(this.Authority, this.Handler);
            var expectedUser = new User
            {
                Username = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                Email = "bob@bob.com",
            };

            // act
            var actualUser = await httpClient.AddUserAsync(expectedUser).ConfigureAwait(false);

            // assert
            var userSummaries = await httpClient.GetUserSummariesAsync().ConfigureAwait(false);
            userSummaries.Should().NotBeNull();
            userSummaries.Should().Contain(summary => summary.Id == actualUser.Id && summary.Username == expectedUser.Username && summary.Email == expectedUser.Email);
        }

        [Fact]
        public async Task CanModifyUser()
        {
            // arrange
            var httpClient = new UsersHttpClient(this.Authority, this.Handler);
            var originalUser = new User
            {
                Username = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                Password = "password4bob",
                Email = "bob@bob.com",
                PhoneNumber = "123456789",
                Roles = { "admin" },
            };

            var expectedUser = new User
            {
                Username = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                Password = "password4superbob",
                Email = "superbob@superbob.com",
                PhoneNumber = "987654321",
                Roles = { "auth_admin", "user_admin" },
            };

            var initialUser = await httpClient.AddUserAsync(originalUser).ConfigureAwait(false);

            // act
            var actualUser = await httpClient.ModifyUserAsync(expectedUser, originalUser.Username).ConfigureAwait(false);

            // assert
            actualUser.Should().NotBeNull();
            actualUser.Should().BeEquivalentTo(expectedUser, options => options.Excluding(user => user.Id).Excluding(user => user.Password));
            actualUser.Id.Should().Be(initialUser.Id);
        }

        [Fact]
        public async Task CanRemoveUser()
        {
            // arrange
            var httpClient = new UsersHttpClient(this.Authority, this.Handler);
            var user = new User
            {
                Username = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
            };

            await httpClient.AddUserAsync(user).ConfigureAwait(false);

            // act
            await httpClient.RemoveUserAsync(user.Username).ConfigureAwait(false);

            // assert
            var userSummaries = await httpClient.GetUserSummariesAsync().ConfigureAwait(false);
            userSummaries.Should().NotBeNull();
            userSummaries.Should().NotContain(summary => summary.Username == user.Username);
        }

        [Fact]
        public async Task CanUseUser()
        {
            // arrange
            var httpClient = new UsersHttpClient(this.Authority, this.Handler);
            var user = new User
            {
                Username = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                Password = "password",
            };

            await httpClient.AddUserAsync(user).ConfigureAwait(false);

            // act
            var automation = new BrowserAutomation(user.Username, user.Password);
            var browser = new Browser(automation);
            var options = new OidcClientOptions
            {
                Authority = this.Authority,
                ClientId = "auth_console",
                RedirectUri = $"http://127.0.0.1:{browser.Port}",
                Scope = "openid profile auth_api offline_access",
                FilterClaims = false,
                Browser = browser,
            };

            var oidcClient = new OidcClient(options);
            var result = await oidcClient.LoginAsync(new LoginRequest()).ConfigureAwait(false);

            // assert
            result.IsError.Should().BeFalse();
        }

        [Fact]
        public void CannotAddInvalidUser()
        {
            // arrange
            var httpClient = new UsersHttpClient(this.Authority, this.Handler);
            var user = new User();

            // act
            Func<Task> func = async () => await httpClient.AddUserAsync(user).ConfigureAwait(false);

            // assert
            func.Should().Throw<HttpException>();
        }

        [Fact]
        public async Task CannotAddDuplicateUser()
        {
            // arrange
            var httpClient = new UsersHttpClient(this.Authority, this.Handler);
            var user = new User
            {
                Username = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
            };

            await httpClient.AddUserAsync(user).ConfigureAwait(false);

            // act
            Func<Task> func = async () => await httpClient.AddUserAsync(user).ConfigureAwait(false);

            // assert
            func.Should().Throw<HttpException>().And.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}