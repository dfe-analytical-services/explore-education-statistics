#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class SubjectRepository : ISubjectRepository
{
    private readonly StatisticsDbContext _context;

    public SubjectRepository(StatisticsDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> FindPublicationIdForSubject(Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ReleaseSubject
            .Where(rs => rs.SubjectId == subjectId)
            .Select(rs => (Guid?) rs.ReleaseVersion.PublicationId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}
