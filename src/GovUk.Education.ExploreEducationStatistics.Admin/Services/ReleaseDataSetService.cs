#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseDataSetService(
    StatisticsDbContext statisticsDbContext,
    IFootnoteRepository footnoteRepository,
    ReleaseDataSetService.SubjectDeleter? subjectDeleter = null
) : IReleaseDataSetService
{
    private readonly SubjectDeleter _subjectDeleter = subjectDeleter ?? new SubjectDeleter();

    public async Task Delete(Guid releaseVersionId, Guid subjectId, bool softDeleteOrphanedDataSet = true)
    {
        await DeleteReleaseSubjectIfExists(releaseVersionId: releaseVersionId, subjectId: subjectId);
        await footnoteRepository.DeleteFootnotesBySubject(releaseVersionId: releaseVersionId, subjectId: subjectId);
        await DeleteDataSetIfOrphaned(
            releaseVersionId: releaseVersionId,
            subjectId: subjectId,
            softDelete: softDeleteOrphanedDataSet
        );
    }

    public async Task DeleteAll(Guid releaseVersionId, bool softDeleteOrphanedDataSets = true)
    {
        var subjectIds = await statisticsDbContext
            .ReleaseSubject.Where(rs => rs.ReleaseVersionId == releaseVersionId)
            .Select(rs => rs.SubjectId)
            .ToListAsync();

        foreach (var subjectId in subjectIds)
        {
            await Delete(
                releaseVersionId: releaseVersionId,
                subjectId: subjectId,
                softDeleteOrphanedDataSet: softDeleteOrphanedDataSets
            );
        }
    }

    public async Task DeleteSoftDeleted(Guid releaseVersionId)
    {
        var subjectIds = await statisticsDbContext
            .Subject.IgnoreQueryFilters()
            .Where(s => s.SoftDeleted && s.SoftDeletedReleaseVersionId == releaseVersionId)
            .Select(s => s.Id)
            .ToListAsync();

        foreach (var subjectId in subjectIds)
        {
            await HardDelete(subjectId);
        }
    }

    private async Task DeleteReleaseSubjectIfExists(Guid releaseVersionId, Guid subjectId)
    {
        var releaseSubject = await statisticsDbContext.ReleaseSubject.FirstOrDefaultAsync(rs =>
            rs.ReleaseVersionId == releaseVersionId && rs.SubjectId == subjectId
        );
        if (releaseSubject != null)
        {
            statisticsDbContext.Remove(releaseSubject);
            // Immediately save context as we will need to re-check
            // how many ReleaseSubjects are attached to the Subject
            // later when determining if the Subject has been orphaned.
            await statisticsDbContext.SaveChangesAsync();
        }
    }

    private async Task DeleteDataSetIfOrphaned(Guid releaseVersionId, Guid subjectId, bool softDelete)
    {
        if (await statisticsDbContext.ReleaseSubject.AnyAsync(rs => rs.SubjectId == subjectId))
        {
            return;
        }

        var subject = await statisticsDbContext
            .Subject.IgnoreQueryFilters()
            .SingleOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
        {
            return;
        }

        if (softDelete)
        {
            subject.SoftDeleted = true;
            subject.SoftDeletedReleaseVersionId = releaseVersionId;
            subject.SoftDeletedDataStorageVersion = DataStorageVersion.StatsDB;
            await statisticsDbContext.SaveChangesAsync();
            return;
        }

        await HardDelete(subjectId);
    }

    private async Task HardDelete(Guid subjectId)
    {
        await footnoteRepository.DeleteFootnoteLinksBySubject(subjectId);
        _subjectDeleter.Delete(subjectId, statisticsDbContext);
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
