using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api
{
    [ExcludeFromCodeCoverage] 
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}