namespace SampleMvcApp
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    /*  NOTE (Cameron): This sample demonstrates the code required to run a hybrid MVC app which allows for offline_access scope.  */

    public class Program
    {
        public static void Main(string[] args) => WebHost.CreateDefaultBuilder(args).UseUrls("http://+:5009").UseStartup<Startup>().Build().Run();
    }
}