#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
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
        ContentDbContext contentDbContext)
    {
        _statisticsDbContext = statisticsDbContext;
        _contentDbContext = contentDbContext;
    }

    public async Task<Either<ActionResult, ReleaseSubject>> Find(Guid subjectId, Guid? releaseId = null)
    {
        return await (
                releaseId.HasValue
                    ? _statisticsDbContext.ReleaseSubject.FirstOrDefaultAsync(
                        rs => rs.ReleaseId == releaseId && rs.SubjectId == subjectId
                    )
                    : FindForLatestPublishedVersion(subjectId)
            )
            .OrNotFound();
    }

    public async Task<ReleaseSubject?> FindForLatestPublishedVersion(Guid subjectId)
    {
        // Find all versions of a Release that this Subject is linked to.
        var releaseSubjects = await _statisticsDbContext
            .ReleaseSubject
            .AsNoTracking()
            .Where(releaseSubject => releaseSubject.SubjectId == subjectId)
            .ToListAsync();

        var releaseIdsLinkedToSubject = releaseSubjects
            .Select(releaseSubject => releaseSubject.ReleaseId)
            .ToList();

        // Find all versions of the Release that this Subject is linked to that are currently live
        // or in the past have been live before being amended. Order them by Version to get the latest
        // Live one at the end of the list.
        var latestLiveReleaseVersionLinkedToSubject = _contentDbContext
            .Releases
            .AsNoTracking()
            .Where(release => releaseIdsLinkedToSubject.Contains(release.Id))
            .ToList()
            .Where(release => release.Live)
            .MaxBy(release => release.Version);

        if (latestLiveReleaseVersionLinkedToSubject == null)
        {
            return null;
        }

        // Finally, now that we have identified the latest Release version linked to this Subject, return the
        // appropriate ReleaseSubject record.
        return releaseSubjects
            .SingleOrDefault(releaseSubject => releaseSubject.ReleaseId == latestLiveReleaseVersionLinkedToSubject.Id);
    }
}
