using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests;

public static class HostBuilderTestExtensions
{
    private const string DevelopmentStorageConnectionString =
        "DefaultEndpointsProtocol=http;AccountName=account;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://data-storage:10000/devstoreaccount1;";

    public static IHostBuilder ConfigureTestAppConfiguration(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureAppConfiguration(s =>
        {
            var configuration = new Dictionary<string, string>
            {
                { "PublicDataDbExists", "false" },
                { "App:PrivateStorageConnectionString", DevelopmentStorageConnectionString },
                { "App:NotifierStorageConnectionString", DevelopmentStorageConnectionString },
                { "App:PublicStorageConnectionString", DevelopmentStorageConnectionString },
                { "App:PublisherStorageConnectionString", DevelopmentStorageConnectionString },
            };

            s.AddInMemoryCollection(configuration!).Build();
        });
    }
}
