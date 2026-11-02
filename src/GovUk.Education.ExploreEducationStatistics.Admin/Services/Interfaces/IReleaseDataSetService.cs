#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseDataSetService
{
    Task Delete(Guid releaseVersionId, Guid subjectId, bool softDeleteOrphanedDataSet = true);

    Task DeleteAll(Guid releaseVersionId, bool softDeleteOrphanedDataSets = true);

    /// <summary>
    /// Deletes the data sets that were soft deleted when they were removed from the release version.
    /// </summary>
    Task DeleteSoftDeleted(Guid releaseVersionId);
}
