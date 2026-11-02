#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseDataSetService(
    StatisticsDbContext statisticsDbContext,
    IFootnoteRepository footnoteRepository,
    IStorageDataSetResolver storageDataSetResolver
) : IReleaseDataSetService
{
    public async Task Delete(Guid releaseVersionId, Guid subjectId, bool softDeleteOrphanedDataSet = true)
    {
        await DeleteReleaseSubjectIfExists(releaseVersionId: releaseVersionId, subjectId: subjectId);
        await footnoteRepository.DeleteFootnotesBySubject(releaseVersionId: releaseVersionId, subjectId: subjectId);
        await DeleteDataSetIfOrphaned(subjectId: subjectId, softDelete: softDeleteOrphanedDataSet);
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

    private async Task DeleteDataSetIfOrphaned(Guid subjectId, bool softDelete)
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

        var dataSet = await storageDataSetResolver.Resolve(subjectId);

        if (softDelete)
        {
            await dataSet.SoftDelete();
            return;
        }

        await dataSet.Delete();

        statisticsDbContext.Subject.Remove(subject);
        await statisticsDbContext.SaveChangesAsync();
    }
}
