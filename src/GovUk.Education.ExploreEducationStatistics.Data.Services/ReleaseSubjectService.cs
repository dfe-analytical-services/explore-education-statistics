#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class ReleaseSubjectService : IReleaseSubjectService
{
    private readonly ContentDbContext _contentDbContext;

    public ReleaseSubjectService(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<Either<ActionResult, ReleaseFile>> Find(Guid subjectId, Guid? releaseVersionId = null)
    {
        return await (
            releaseVersionId.HasValue
                ? _contentDbContext
                    .ReleaseFiles.Include(rf => rf.File)
                    .FirstOrDefaultAsync(rf =>
                        rf.ReleaseVersionId == releaseVersionId
                        && rf.File.SubjectId == subjectId
                        && rf.File.Type == FileType.Data
                    )
                : FindForLatestPublishedVersion(subjectId)
        ).OrNotFound();
    }

    public async Task<ReleaseFile?> FindForLatestPublishedVersion(Guid subjectId)
    {
        // Find all versions of a Release that this Subject is linked to.
        var releaseFiles = await _contentDbContext
            .ReleaseFiles.AsNoTracking()
            .Include(rf => rf.File)
            .Where(rf => rf.File.SubjectId == subjectId && rf.File.Type == FileType.Data)
            .ToListAsync();

        var releaseVersionIdsLinkedToSubject = releaseFiles.Select(rf => rf.ReleaseVersionId).ToList();

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
        // appropriate ReleaseFile record.
        return releaseFiles.SingleOrDefault(rf => rf.ReleaseVersionId == latestLiveReleaseVersionLinkedToSubject.Id);
    }
}
