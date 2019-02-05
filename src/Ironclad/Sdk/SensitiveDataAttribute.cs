// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Sdk
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public sealed class SensitiveDataAttribute : Attribute
    {
    }
}
