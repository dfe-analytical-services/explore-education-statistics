#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseDataSetService
{
    Task Delete(Guid releaseVersionId, Guid subjectId, bool softDeleteOrphanedDataSet = true);

    Task DeleteAll(Guid releaseVersionId, bool softDeleteOrphanedDataSets = true);
}
