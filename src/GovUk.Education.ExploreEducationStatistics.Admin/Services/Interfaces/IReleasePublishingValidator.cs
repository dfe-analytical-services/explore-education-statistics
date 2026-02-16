using GovUk.Education.ExploreEducationStatistics.Content.Model;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleasePublishingValidator
{
    Task<bool> IsMissingUpdatedApiDataSet(
        ReleaseVersion releaseVersion,
        IList<File> dataFileUploads,
        CancellationToken cancellationToken = default
    );
}
