using GovUk.Education.ExploreEducationStatistics.Publisher;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder().ConfigureFunctionsWebApplication().ConfigurePublisherHostBuilder().Build();

await host.RunAsync();
