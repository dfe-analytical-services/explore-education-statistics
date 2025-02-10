using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureProcessorHostBuilder()
    .Build();

host.Run();
