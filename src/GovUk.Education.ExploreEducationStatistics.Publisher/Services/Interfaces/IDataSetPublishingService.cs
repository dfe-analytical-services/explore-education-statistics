using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IDataSetPublishingService
{
    Task PublishDataSets(Guid[] releaseVersionIds);
}
