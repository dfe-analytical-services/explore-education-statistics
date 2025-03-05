using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class HostExtensions
{
    public static IHost Execute(this IHost host, Action<IHost> action)
    {
        action(host);
        return host;
    }
}
