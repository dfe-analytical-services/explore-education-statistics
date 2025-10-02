using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using Microsoft.Extensions.Hosting;

await new HostBuilder().InitialiseSerilog().BuildHost().RunAsync();
