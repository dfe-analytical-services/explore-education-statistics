using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublishingService
{
    Task PublishStagedReleaseContent();

    Task PublishMethodologyFiles(Guid methodologyId);

    Task PublishMethodologyFilesIfApplicableForRelease(Guid releaseVersionId);

    Task PublishReleaseFiles(Guid releaseVersionId);
}
