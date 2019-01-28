// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Console.Persistence
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Microsoft.AspNetCore.DataProtection;

    public class CommandDataRepository : ICommandDataRepository
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(CommandData));

        private readonly CustomFileSystemXmlRepository innerRepository = new CustomFileSystemXmlRepository();
        private readonly IDataProtector protector;

        public CommandDataRepository(IDataProtectionProvider provider)
        {
            this.protector = provider.CreateProtector("auth_console");
        }

        public CommandData GetCommandData()
        {
            var element = this.innerRepository.GetAllElements().FirstOrDefault();
            if (element == null)
            {
                return null;
            }

            var data = (CommandData)Serializer.Deserialize(element.CreateReader());

            return new CommandData
            {
                Authority = data.Authority,
                AccessToken = this.protector.Unprotect(data.AccessToken),
                AccessTokenExpiration = data.AccessTokenExpiration,
                RefreshToken = this.protector.Unprotect(data.RefreshToken),
            };
        }

        public void SetCommandData(CommandData commandData)
        {
            if (commandData == null)
            {
                var filename = Path.Combine(this.innerRepository.Directory.FullName, "command-data.xml");
                File.Delete(filename); // won't throw if the file doesn't exist
                return;
            }

            ////var authority = this.protector.Protect(commandData.Authority);
            var data = new CommandData
            {
                Authority = commandData.Authority,
                AccessToken = this.protector.Protect(commandData.AccessToken),
                AccessTokenExpiration = commandData.AccessTokenExpiration,
                RefreshToken = this.protector.Protect(commandData.RefreshToken),
            };

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                Serializer.Serialize(streamWriter, data);

                var xml = XElement.Parse(Encoding.ASCII.GetString(memoryStream.ToArray()));
                this.innerRepository.StoreElement(xml, "command-data");
            }
        }
    }
}
