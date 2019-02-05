// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Ironclad.Models
{
    using System.Collections.Generic;

    public class WebsiteSettings
    {
        public string Styles { get; set; } = "css/default.css";

        public string Logo { get; set; } = "img/fingerprint.svg";

        // HACK (Cameron): This really doesn't belong here.
        public IEnumerable<string> RestrictedDomains { get; set; }

        public bool? ShowLoginScreen { get; set; }
    }
}
