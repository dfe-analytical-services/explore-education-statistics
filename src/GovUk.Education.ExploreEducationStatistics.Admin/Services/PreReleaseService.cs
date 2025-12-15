using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PreReleaseService(IOptions<PreReleaseAccessOptions> options) : IPreReleaseService
{
    public PreReleaseWindow GetPreReleaseWindow(ReleaseVersion releaseVersion)
    {
        if (!releaseVersion.PublishScheduled.HasValue)
        {
            throw new ArgumentException("Release version has no PublishScheduled value", nameof(releaseVersion));
        }

        var publishScheduled = releaseVersion.PublishScheduled.Value;

        return new PreReleaseWindow { Start = GetStartTime(publishScheduled), ScheduledPublishDate = publishScheduled };
    }

    public PreReleaseWindowStatus GetPreReleaseWindowStatus(ReleaseVersion releaseVersion, DateTimeOffset referenceTime)
    {
        if (releaseVersion.Live)
        {
            return new PreReleaseWindowStatus { Access = PreReleaseAccess.After };
        }

        if (!releaseVersion.PublishScheduled.HasValue)
        {
            return new PreReleaseWindowStatus { Access = PreReleaseAccess.NoneSet };
        }

        var publishScheduled = releaseVersion.PublishScheduled.Value;
        var startTime = GetStartTime(publishScheduled);

        return new PreReleaseWindowStatus
        {
            Start = GetStartTime(publishScheduled),
            ScheduledPublishDate = publishScheduled,
            Access = GetAccess(releaseVersion, startTime, referenceTime),
        };
    }

    private DateTimeOffset GetStartTime(DateTimeOffset publishScheduled)
    {
        return publishScheduled.AddMinutes(-options.Value.AccessWindow.MinutesBeforeReleaseTimeStart);
    }

    private static PreReleaseAccess GetAccess(
        ReleaseVersion releaseVersion,
        DateTimeOffset startTime,
        DateTimeOffset referenceTime
    )
    {
        if (
            !releaseVersion.PublishScheduled.HasValue
            || releaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved
        )
        {
            return PreReleaseAccess.NoneSet;
        }

        if (referenceTime < startTime)
        {
            return PreReleaseAccess.Before;
        }

        return releaseVersion.Live ? PreReleaseAccess.After : PreReleaseAccess.Within;
    }
}
