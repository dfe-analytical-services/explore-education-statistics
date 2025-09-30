using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IMethodologyService
{
    Task<MethodologyVersion> Get(Guid methodologyVersionId);

    Task<List<MethodologyVersion>> GetLatestVersionByRelease(ReleaseVersion releaseVersion);

    Task<List<File>> GetFiles(Guid methodologyVersionId, params FileType[] types);

    Task Publish(MethodologyVersion methodologyVersion);

    Task<bool> IsBeingPublishedAlongsideRelease(
        MethodologyVersion methodologyVersion,
        ReleaseVersion releaseVersion
    );
}
