// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace SampleTests
{
    using Ironclad.Tests.Sdk;
    using Xunit;

    // LINK (Cameron): https://xunit.github.io/docs/shared-context.html
    // NOTE (cameron): Fixtures can be shared across assemblies, but collection definitions must be in the same assembly as the test that uses them.
    [CollectionDefinition(nameof(AuthenticationTest))]
    public class SampleTestCollection : ICollectionFixture<SampleAuthenticationFixture>
    {
        // This class has no code, and is never created. Its purpose is simply to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
    }
}
