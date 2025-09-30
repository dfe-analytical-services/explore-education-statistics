#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class ReleaseSubjectService : IReleaseSubjectService
{
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly ContentDbContext _contentDbContext;

    public ReleaseSubjectService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext
    )
    {
        _statisticsDbContext = statisticsDbContext;
        _contentDbContext = contentDbContext;
    }

    public async Task<Either<ActionResult, ReleaseSubject>> Find(
        Guid subjectId,
        Guid? releaseVersionId = null
    )
    {
        return await (
            releaseVersionId.HasValue
                ? _statisticsDbContext.ReleaseSubject.FirstOrDefaultAsync(rs =>
                    rs.ReleaseVersionId == releaseVersionId && rs.SubjectId == subjectId
                )
                : FindForLatestPublishedVersion(subjectId)
        ).OrNotFound();
    }

    public async Task<ReleaseSubject?> FindForLatestPublishedVersion(Guid subjectId)
    {
        // Find all versions of a Release that this Subject is linked to.
        var releaseSubjects = await _statisticsDbContext
            .ReleaseSubject.AsNoTracking()
            .Where(releaseSubject => releaseSubject.SubjectId == subjectId)
            .ToListAsync();

        var releaseVersionIdsLinkedToSubject = releaseSubjects
            .Select(releaseSubject => releaseSubject.ReleaseVersionId)
            .ToList();

        // Find all versions of the Release that this Subject is linked to that are currently live
        // or in the past have been live before being amended. Order them by Version to get the latest
        // Live one at the end of the list.
        var latestLiveReleaseVersionLinkedToSubject = _contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Where(releaseVersion => releaseVersionIdsLinkedToSubject.Contains(releaseVersion.Id))
            .ToList()
            .Where(releaseVersion => releaseVersion.Live)
            .MaxBy(releaseVersion => releaseVersion.Version);

        if (latestLiveReleaseVersionLinkedToSubject == null)
        {
            return null;
        }

        // Finally, now that we have identified the latest Release version linked to this Subject, return the
        // appropriate ReleaseSubject record.
        return releaseSubjects.SingleOrDefault(releaseSubject =>
            releaseSubject.ReleaseVersionId == latestLiveReleaseVersionLinkedToSubject.Id
        );
    }
}
