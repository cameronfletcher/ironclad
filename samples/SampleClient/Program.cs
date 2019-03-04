namespace SampleClient
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;

    /*  NOTE (Cameron): This sample demonstrates the code required to make a server-to-server API call with no user involvement.  */

    internal static class Program
    {
        // NOTE (Cameron): In this sample I am not disposing of any of the clients. You should determine for yourselves if/when you need to do this in your code.
        // LINK (Cameron): https://stackoverflow.com/questions/37157164/best-practice-for-use-httpclient
        public static async Task Main(string[] args)
        {
            // make a discovery request to the auth server in order to discover the token endpoint
            // this call typically only needs to be done every time the auth server fundamentally changes
            var discoveryClient = new DiscoveryClient("http://localhost:5005/") { Policy = new DiscoveryPolicy { ValidateIssuerName = false } };
            var discoveryResponse = await discoveryClient.GetAsync();

            // make a token request to the token endpoint (this is the server-to-server auth endpoint eg. no user input required/permitted)
            // this call needs to be done every time a new token is required - a refresh token could also be requested for this purpose
            var tokenClient = new TokenClient(discoveryResponse.TokenEndpoint, "sample_client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("sample_api");

            // now we're into standard .NET HTTP calls
            // ensure the request has the token in the header - this should be set for a single HTTP client which should be reused for all required calls
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            using (var apiResponse = await apiClient.GetAsync("http://localhost:5007/api"))
            {
                if (apiResponse.IsSuccessStatusCode)
                {
                    var content = await apiResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(content);
                }
                else
                {
                    Console.WriteLine($"{(int)apiResponse.StatusCode} {apiResponse.ReasonPhrase}");
                }
            }
        }
    }
}
