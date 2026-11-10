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

    public async Task<IReadOnlyDictionary<Guid, IStorageDataSet>> ResolveAll(Guid releaseVersionId)
    {
        var dataSets = new Dictionary<Guid, IStorageDataSet>();

        foreach (var subjectId in await ListSubjectIds(releaseVersionId))
        {
            var dataSet = await storageDataSetResolver.TryResolve(subjectId);

            if (dataSet != null)
            {
                dataSets[subjectId] = dataSet;
            }
        }

        return dataSets;
    }

    public async Task DeleteAll(
        Guid releaseVersionId,
        IReadOnlyDictionary<Guid, IStorageDataSet> dataSets,
        bool softDeleteOrphanedDataSets = true
    )
    {
        foreach (var subjectId in await ListSubjectIds(releaseVersionId))
        {
            await DeleteReleaseSubjectIfExists(releaseVersionId: releaseVersionId, subjectId: subjectId);
            await footnoteRepository.DeleteFootnotesBySubject(releaseVersionId: releaseVersionId, subjectId: subjectId);
            await DeleteDataSetIfOrphaned(
                subjectId: subjectId,
                softDelete: softDeleteOrphanedDataSets,
                dataSet: dataSets.GetValueOrDefault(subjectId)
            );
        }
    }

    private async Task<List<Guid>> ListSubjectIds(Guid releaseVersionId)
    {
        return await statisticsDbContext
            .ReleaseSubject.Where(rs => rs.ReleaseVersionId == releaseVersionId)
            .Select(rs => rs.SubjectId)
            .ToListAsync();
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

    private async Task DeleteDataSetIfOrphaned(Guid subjectId, bool softDelete, IStorageDataSet? dataSet = null)
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

        dataSet ??= await storageDataSetResolver.Resolve(subjectId);

        await dataSet.Delete(softDelete: softDelete);

        if (softDelete)
        {
            return;
        }

        statisticsDbContext.Subject.Remove(subject);
        await statisticsDbContext.SaveChangesAsync();
    }
}
