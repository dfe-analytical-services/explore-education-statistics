#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IReleaseSubjectRepository
{
    Task<IReadOnlyList<ReleaseSubject>> FindAll(
        Guid releaseVersionId,
        Func<IQueryable<ReleaseSubject>, IQueryable<ReleaseSubject>>? queryExtender = null
    );
}
