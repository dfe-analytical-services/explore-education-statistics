#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

public interface IStorageDataSetResolver
{
    Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default);

    IStorageDataSet Resolve(File dataFile);

    IStorageDataSet Resolve(Guid subjectId, DataStorageVersion dataStorageVersion);

    Task<IStorageDataSet?> TryResolve(Guid subjectId, CancellationToken cancellationToken = default);
}
