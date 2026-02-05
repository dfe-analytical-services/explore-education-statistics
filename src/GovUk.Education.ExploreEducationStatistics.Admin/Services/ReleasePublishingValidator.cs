using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleasePublishingValidator(IDataSetService dataSetService) : IReleasePublishingValidator
{
    public async Task<bool> IsMissingUpdatedApiDataSet(
        ReleaseVersion releaseVersion,
        IList<File> dataFileUploads,
        CancellationToken cancellationToken = default
    )
    {
        if (IsNewRelease(releaseVersion))
        {
            return false;
        }

        if (dataFileUploads.Count == 0)
        {
            return false;
        }

        var existingDataSetsForPublication = await dataSetService.ListDataSets(
            releaseVersion.Release.PublicationId,
            cancellationToken
        );
        var existingDataSets = existingDataSetsForPublication.Right;

        if (existingDataSets.Count == 0)
        {
            return false;
        }

        var draftVersionFileIds = existingDataSets
            .Where(ds =>
                ds.DraftVersion is not null
                && ds.DraftVersion.Status is DataSetVersionStatus.Mapping or DataSetVersionStatus.Draft
            )
            .Select(d => d.DraftVersion.File.Id)
            .ToList();

        foreach (var upload in dataFileUploads)
        {
            if (!draftVersionFileIds.Contains((Guid)upload.DataSetFileId) && !releaseVersion.Live)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNewRelease(ReleaseVersion releaseVersion) =>
        releaseVersion.Release.Publication.LatestPublishedReleaseVersionId is null;
}
