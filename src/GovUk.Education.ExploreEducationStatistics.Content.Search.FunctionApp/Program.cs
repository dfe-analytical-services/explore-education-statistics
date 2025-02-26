using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureHostBuilder()
    .Build();

await host.RunAsync();
