#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class SubjectRepository : ISubjectRepository
{
    private readonly ContentDbContext _context;

    public SubjectRepository(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> FindPublicationIdForSubject(Guid subjectId, CancellationToken cancellationToken = default)
    {
        return await _context
            .ReleaseFiles.Where(rf => rf.File.SubjectId == subjectId && rf.File.Type == FileType.Data)
            .Select(rf => (Guid?)rf.ReleaseVersion.Release.PublicationId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}
