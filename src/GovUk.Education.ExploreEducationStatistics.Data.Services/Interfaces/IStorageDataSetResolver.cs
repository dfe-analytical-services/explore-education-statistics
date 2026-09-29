#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

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
    /// As <see cref="Resolve" />, but returns null when no data file is recorded for the subject,
    /// for example because the data set has since been deleted.
    /// </summary>
    Task<IStorageDataSet?> TryResolve(Guid subjectId, CancellationToken cancellationToken = default);
}
