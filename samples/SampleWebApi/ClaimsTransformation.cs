namespace SampleWebApi
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;

    public class ClaimsTransformation : IClaimsTransformation
    {
        // this demonstrates claims transformation where we take the claims off the wire and transform them with information that only we know
        // what follows is a trivial example of this transformation; typically you would use information for some persistence layer
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // add the claim 'now' for every caller to demonstrate the functionality
            var claims = new List<Claim>(principal.Claims)
            {
                new Claim("now", DateTime.UtcNow.ToString()),
            };

            // if the claims are coming from a client (clients have no name claim by default) and not on behalf of a user then we introduce the name claim
            if (!principal.HasClaim(claim => claim.Type == "name"))
            {
                claims.Add(new Claim("name", principal.FindFirst("client_id").Value));
            }

            // there is no other valid constructor method for the claims identity; all others will come back and bite you at some point
            var identity = new ClaimsIdentity(claims, principal.Identity.AuthenticationType, "name", "role");

            return Task.FromResult(new ClaimsPrincipal(identity));
        }
    }
}
