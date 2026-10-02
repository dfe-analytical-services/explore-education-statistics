#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

public interface IStorageDataSetResolver
{
    /// <summary>
    /// Returns an <see cref="IStorageDataSet" /> bound to the data set (Subject) identified by
    /// <paramref name="subjectId" />. The Subject is not checked for existence.
    ///
    /// Resolving the same subject again within the current request scope returns the same instance. That instance
    /// is only valid for the lifetime of the scope and must not be cached beyond it.
    /// </summary>
    Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// As <see cref="Resolve(Guid, CancellationToken)" />, but reads the <see cref="DataStorageVersion" /> from a
    /// data <see cref="File" /> the caller has already loaded, so the data file is not looked up again.
    /// <paramref name="dataFile" /> must be of type <see cref="FileType.Data" /> and linked to a subject.
    /// </summary>
    IStorageDataSet Resolve(File dataFile);

    /// <summary>
    /// As <see cref="Resolve(Guid, CancellationToken)" />, but returns null when no data file is recorded for the
    /// subject, for example because the data set has since been deleted.
    /// </summary>
    Task<IStorageDataSet?> TryResolve(Guid subjectId, CancellationToken cancellationToken = default);
}
