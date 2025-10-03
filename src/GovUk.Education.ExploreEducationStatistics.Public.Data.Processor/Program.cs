using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder().ConfigureFunctionsWebApplication().ConfigureProcessorHostBuilder().Build();

await host.RunAsync();
