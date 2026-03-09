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

        var allPublicationDataSets = existingDataSetsForPublication.Right;
        var dataSetsNotUpdatedInThisReleaseVersion = new List<DataSetSummaryViewModel>(allPublicationDataSets);
        var dataUploadsNotAssociatedToApiDataSetCount = dataFileUploads.Count;

        if (allPublicationDataSets.Count == 0)
        {
            return false;
        }

        foreach (var dataSet in allPublicationDataSets)
        {
            // If the API data set has been updated in the current release, it will be in a transitory state.
            // If the condition is met, since one upload is needed to update one API data set, we remove one of each.
            // If both lists are empty at the end, it means that each upload has been correctly associated to a new version of an API data set, and no warning is shown.
            if (
                dataSet.DraftVersion?.Status is DataSetVersionStatus.Mapping or DataSetVersionStatus.Draft
                && dataSet.DraftVersion.ReleaseVersion.Id == releaseVersion.Id
            )
            {
                dataSetsNotUpdatedInThisReleaseVersion.Remove(dataSet);
                dataUploadsNotAssociatedToApiDataSetCount--;
            }
        }

        return dataSetsNotUpdatedInThisReleaseVersion.Count > 0 && dataUploadsNotAssociatedToApiDataSetCount > 0;
    }

    private static bool IsNewRelease(ReleaseVersion releaseVersion) =>
        releaseVersion.Release.Publication.LatestPublishedReleaseVersionId is null;
}
