using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public interface IConfigurationProvider
    {
        IConfigurationRoot Get(ExecutionContext context);
    }
}
