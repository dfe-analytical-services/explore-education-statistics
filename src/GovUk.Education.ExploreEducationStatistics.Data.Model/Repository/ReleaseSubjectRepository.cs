#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class ReleaseSubjectRepository : IReleaseSubjectRepository
{
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly IFootnoteRepository _footnoteRepository;
    private readonly SubjectDeleter _subjectDeleter;

    public ReleaseSubjectRepository(
        StatisticsDbContext statisticsDbContext,
        IFootnoteRepository footnoteRepository,
        SubjectDeleter? subjectDeleter = null)
    {
        _statisticsDbContext = statisticsDbContext;
        _footnoteRepository = footnoteRepository;
        _subjectDeleter = subjectDeleter ?? new SubjectDeleter();
    }

    public async Task<IReadOnlyList<ReleaseSubject>> FindAll(
        Guid releaseVersionId,
        Func<IQueryable<ReleaseSubject>, IQueryable<ReleaseSubject>>? queryExtender = null)
    {
        IQueryable<ReleaseSubject> query = _statisticsDbContext.ReleaseSubject
            .Where(rs => rs.ReleaseVersionId == releaseVersionId);

        if (queryExtender is not null)
        {
            query = queryExtender.Invoke(query);
        }

        return await query
            .ToListAsync();
    }

    public async Task DeleteAllReleaseSubjects(Guid releaseVersionId, bool softDeleteOrphanedSubjects = true)
    {
        await _statisticsDbContext.ReleaseSubject
            .Where(rs => rs.ReleaseVersionId == releaseVersionId)
            .Select(rs => rs.SubjectId)
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async subjectId =>
            {
                await DeleteReleaseSubject(releaseVersionId: releaseVersionId,
                    subjectId: subjectId,
                    softDeleteOrphanedSubject: softDeleteOrphanedSubjects);
            });
    }

    public async Task DeleteReleaseSubject(Guid releaseVersionId,
        Guid subjectId,
        bool softDeleteOrphanedSubject = true)
    {
        await DeleteReleaseSubjectIfExists(releaseVersionId: releaseVersionId, subjectId: subjectId);
        await _footnoteRepository.DeleteFootnotesBySubject(releaseVersionId: releaseVersionId,
            subjectId: subjectId);
        await DeleteSubjectIfOrphaned(subjectId: subjectId, softDeleteOrphanedSubject);
    }

    private async Task DeleteReleaseSubjectIfExists(Guid releaseVersionId, Guid subjectId)
    {
        var releaseSubject = await _statisticsDbContext.ReleaseSubject
            .FirstOrDefaultAsync(rs => rs.ReleaseVersionId == releaseVersionId && rs.SubjectId == subjectId);
        if (releaseSubject != null)
        {
            _statisticsDbContext.Remove(releaseSubject);
            // Immediately save context as we will need to re-check
            // how many ReleaseSubjects are attached to the Subject
            // later when determining if the Subject has been orphaned.
            await _statisticsDbContext.SaveChangesAsync();
        }
    }

    private async Task DeleteSubjectIfOrphaned(Guid subjectId, bool softDelete)
    {
        if (await _statisticsDbContext.ReleaseSubject.AnyAsync(rs => rs.SubjectId == subjectId))
        {
            return;
        }

        if (softDelete)
        {
            var subject = await _statisticsDbContext.Subject.FirstOrDefaultAsync(s => s.Id == subjectId);
            if (subject != null)
            {
                _statisticsDbContext.Subject.Update(subject);
                subject.SoftDeleted = true;
            }
        }
        else
        {
            _subjectDeleter.Delete(subjectId, _statisticsDbContext);
        }

        await _statisticsDbContext.SaveChangesAsync();
    }

    // Separate this into a separate class
    // for ease of mocking in tests.
    public class SubjectDeleter
    {
        public virtual void Delete(Guid subjectId, StatisticsDbContext context)
        {
            // Use raw delete as EF can't correctly figure out how to cascade the delete, whilst the database can.
            // N.B. This delete will be slow if there are a large number of observations but this is only
            // executed by the tests when the theme is torn down so ensure files used are < 1000 rows.
            context.Database.ExecuteSqlInterpolated($"DELETE FROM Subject WHERE Id = {subjectId}");
        }
    }
}
