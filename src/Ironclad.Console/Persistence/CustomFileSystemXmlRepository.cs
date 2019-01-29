// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Console.Persistence
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;
    using Microsoft.AspNetCore.DataProtection.Repositories;
    using Microsoft.Extensions.Logging.Abstractions;

    public class CustomFileSystemXmlRepository : FileSystemXmlRepository
    {
        private const string RepositoryFolderName = ".ironclad";

        public CustomFileSystemXmlRepository()
            : base(GetDefaultDataStorageDirectory(), NullLoggerFactory.Instance)
        {
        }

        public override void StoreElement(XElement element, string friendlyName)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (!IsSafeFilename(friendlyName))
            {
                var newFriendlyName = Guid.NewGuid().ToString();
                friendlyName = newFriendlyName;
            }

            this.StoreElementCore(element, friendlyName);
        }

        // must be non-empty and contain only a-zA-Z0-9, hyphen, and underscore
        private static bool IsSafeFilename(string filename) =>
            !string.IsNullOrEmpty(filename) && filename.All(c => c == '-' || c == '_' || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'));

        // LINK (Cameron): https://github.com/aspnet/DataProtection/blob/dev/src/Microsoft.AspNetCore.DataProtection/Repositories/FileSystemXmlRepository.cs
        private static DirectoryInfo GetDefaultDataStorageDirectory()
        {
            DirectoryInfo directoryInfo;

            // Environment.GetFolderPath returns null if the user profile isn't loaded.
            var localAppDataFromSystemPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var localAppDataFromEnvPath = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            var userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            var homePath = Environment.GetEnvironmentVariable("HOME");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !string.IsNullOrEmpty(localAppDataFromSystemPath))
            {
                // To preserve backwards-compatibility with 1.x, Environment.SpecialFolder.LocalApplicationData
                // cannot take precedence over $LOCALAPPDATA and $HOME/.aspnet on non-Windows platforms
                directoryInfo = GetKeyStorageDirectoryFromBaseAppDataPath(localAppDataFromSystemPath);
            }
            else if (localAppDataFromEnvPath != null)
            {
                directoryInfo = GetKeyStorageDirectoryFromBaseAppDataPath(localAppDataFromEnvPath);
            }
            else if (userProfilePath != null)
            {
                directoryInfo = GetKeyStorageDirectoryFromBaseAppDataPath(Path.Combine(userProfilePath, "AppData", "Local"));
            }
            else if (homePath != null)
            {
                // If LOCALAPPDATA and USERPROFILE are not present but HOME is, it's a good guess that this is a *NIX machine.  Use *NIX conventions for a folder name.
                directoryInfo = new DirectoryInfo(Path.Combine(homePath, RepositoryFolderName));
            }
            else if (!string.IsNullOrEmpty(localAppDataFromSystemPath))
            {
                // Starting in 2.x, non-Windows platforms may use Environment.SpecialFolder.LocalApplicationData
                // but only after checking for $LOCALAPPDATA, $USERPROFILE, and $HOME.
                directoryInfo = GetKeyStorageDirectoryFromBaseAppDataPath(localAppDataFromSystemPath);
            }
            else
            {
                return null;
            }

            Debug.Assert(directoryInfo != null, "The storage directory was not located.");

            try
            {
                directoryInfo.Create(); // throws if we don't have access, e.g., user profile not loaded
                return directoryInfo;
            }
            catch
            {
                return null;
            }
        }

        private static DirectoryInfo GetKeyStorageDirectoryFromBaseAppDataPath(string basePath) => new DirectoryInfo(Path.Combine(basePath, RepositoryFolderName));

        private void StoreElementCore(XElement element, string filename)
        {
            // We're first going to write the file to a temporary location. This way, another consumer
            // won't try reading the file in the middle of us writing it. Additionally, if our process
            // crashes mid-write, we won't end up with a corrupt .xml file.
            this.Directory.Create(); // won't throw if the directory already exists
            var tempFilename = Path.Combine(this.Directory.FullName, Guid.NewGuid().ToString() + ".tmp");
            var finalFilename = Path.Combine(this.Directory.FullName, filename + ".xml");

            try
            {
                using (var tempFileStream = File.OpenWrite(tempFilename))
                {
                    element.Save(tempFileStream);
                }

                File.Copy(tempFilename, finalFilename, true);
            }
            finally
            {
                File.Delete(tempFilename); // won't throw if the file doesn't exist
            }
        }
    }
}
