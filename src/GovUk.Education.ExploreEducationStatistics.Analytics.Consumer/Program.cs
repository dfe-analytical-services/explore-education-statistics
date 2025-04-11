using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureProcessorHostBuilder()
    .Build();

host.Run();
