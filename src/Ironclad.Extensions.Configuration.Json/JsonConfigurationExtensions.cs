// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Ironclad.Settings.Configuration.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for adding <see cref="JsonConfigurationProvider"/>.
    /// </summary>
    public static class JsonConfigurationExtensions
    {
        /// <summary>
        /// Adds the JSON configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in 
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonFileX(this IConfigurationBuilder builder, string path)
        {
            return AddJsonFileX(builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        /// <summary>
        /// Adds the JSON configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in 
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonFileX(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddJsonFileX(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        /// <summary>
        /// Adds the JSON configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in 
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonFileX(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddJsonFileX(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        /// <summary>
        /// Adds a JSON configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
        /// <param name="path">Path relative to the base path stored in 
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonFileX(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("File path must be a non-empty string.", nameof(path));
            }

            return builder.AddJsonFileX(s =>
            {
                s.FileProvider = provider;
                s.Path = path;
                s.Optional = optional;
                s.ReloadOnChange = reloadOnChange;
                s.ResolveFileProvider();
            });
        }

        /// <summary>
        /// Adds a JSON configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonFileX(this IConfigurationBuilder builder, Action<JsonXConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }
}
