#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseDataSetService
{
    Task Delete(Guid releaseVersionId, Guid subjectId, bool softDeleteOrphanedDataSet = true);

    /// <summary>
    /// Resolves the data sets of a release version, keyed by subject id, so that they can still be deleted with
    /// <see cref="DeleteAll"/> after the data files they are resolved from have been deleted.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, IStorageDataSet>> ResolveAll(Guid releaseVersionId);

    Task DeleteAll(
        Guid releaseVersionId,
        IReadOnlyDictionary<Guid, IStorageDataSet> dataSets,
        bool softDeleteOrphanedDataSets = true
    );
}
