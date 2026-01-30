using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleasePublishingValidator(IDataSetService dataSetService) : IReleasePublishingValidator
{
    public async Task<bool> IsMissingUpdatedApiDataSet(
        ReleaseVersion releaseVersion,
        IList<File> dataFiles,
        CancellationToken cancellationToken = default
    )
    {
        // if it's a new/unpublished publication, there's no previous release to check
        if (releaseVersion.Release.Publication.LatestPublishedReleaseVersionId is null)
        {
            return false;
        }

        // if no data files were uploaded, there's no data to associate with an existing API data set
        if (!dataFiles.Any())
        {
            return false;
        }

        // get data sets associated to this publication
        // TODO: Use results pattern
        var existingDataSetsForPublication = await dataSetService.ListDataSets(releaseVersion.Release.PublicationId);

        // if there are fewer uploads than API data sets, this may be intentional (e.g. two API data sets, but only one needs updating)
        if (dataFiles.Count < existingDataSetsForPublication.Right.Count)
        {
            return false;
        }

        // if no new data set version has been associated to this release, add the warning
        if (existingDataSetsForPublication.Right.Any(r => DataSetVersionIsNotAssociatedToRelease(r, releaseVersion.Id)))
        {
            return true;
        }

        return false;
    }

    private static bool DataSetVersionIsNotAssociatedToRelease(
        DataSetSummaryViewModel dataSet,
        Guid releaseVersionId
    ) => // might be wrong logic? Been suggested that this shouldn't work with amendment scenarios when comparing releaseVersionIds
        dataSet.LatestLiveVersion?.ReleaseVersion.Id != releaseVersionId
        && (
            dataSet.DraftVersion is null
            || dataSet.DraftVersion.ReleaseVersion.Id != releaseVersionId
            || dataSet.DraftVersion.Status is not DataSetVersionStatus.Mapping and not DataSetVersionStatus.Draft
        );
}
