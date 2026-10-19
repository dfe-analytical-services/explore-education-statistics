#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface ISubjectRepository
{
    Task<Guid?> FindPublicationIdForSubject(Guid subjectId, CancellationToken cancellationToken = default);
}
