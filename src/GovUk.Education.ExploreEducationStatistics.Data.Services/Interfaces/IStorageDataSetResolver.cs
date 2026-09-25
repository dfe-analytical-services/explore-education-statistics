#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface IStorageDataSetResolver
{
    /// <summary>
    /// Returns an <see cref="IStorageDataSet" /> bound to the data set (Subject) identified by
    /// <paramref name="subjectId" />. The Subject is not checked for existence.
    ///
    /// The returned instance is only valid for the lifetime of the current request scope and must not be cached.
    /// </summary>
    Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default);
}
