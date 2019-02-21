namespace SampleWebApi
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    /*  NOTE (Cameron): This sample demonstrates the code required to secure a web API.  */

    public class Program
    {
        public static void Main(string[] args) => WebHost.CreateDefaultBuilder(args).UseUrls("http://+:5006").UseStartup<Startup>().Build().Run();
    }
}
