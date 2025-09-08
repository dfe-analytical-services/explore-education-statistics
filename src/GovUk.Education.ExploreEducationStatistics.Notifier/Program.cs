using GovUk.Education.ExploreEducationStatistics.Notifier;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder().ConfigureFunctionsWebApplication().ConfigureNotifierHostBuilder().Build();

await host.RunAsync();
