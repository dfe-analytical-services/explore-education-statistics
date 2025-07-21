using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PreReleaseService : IPreReleaseService
{
    private readonly AccessWindowOptions _preReleaseOptions;

    public PreReleaseService(IOptions<PreReleaseAccessOptions> options)
    {
        _preReleaseOptions = options.Value.AccessWindow;
    }

    public PreReleaseWindow GetPreReleaseWindow(ReleaseVersion releaseVersion)
    {
        if (!releaseVersion.PublishScheduled.HasValue)
        {
            throw new ArgumentException("Release version has no PublishScheduled value", nameof(releaseVersion));
        }

        var publishScheduled = releaseVersion.PublishScheduled.Value;

        return new PreReleaseWindow
        {
            Start = GetStartTime(publishScheduled),
            ScheduledPublishDate = publishScheduled
        };
    }

    public PreReleaseWindowStatus GetPreReleaseWindowStatus(ReleaseVersion releaseVersion, DateTime referenceTime)
    {
        if (releaseVersion.Live)
        {
            return new PreReleaseWindowStatus
            {
                Access = PreReleaseAccess.After
            };
        }

        if (!releaseVersion.PublishScheduled.HasValue)
        {
            return new PreReleaseWindowStatus
            {
                Access = PreReleaseAccess.NoneSet
            };
        }

        var publishScheduled = releaseVersion.PublishScheduled.Value;
        var startTime = GetStartTime(publishScheduled);

        return new PreReleaseWindowStatus
        {
            Start = GetStartTime(publishScheduled),
            ScheduledPublishDate = publishScheduled,
            Access = GetAccess(releaseVersion, startTime, referenceTime)
        };
    }

    private DateTime GetStartTime(DateTime publishScheduled)
    {
        return publishScheduled.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeStart);
    }

    private static PreReleaseAccess GetAccess(
        ReleaseVersion releaseVersion,
        DateTime startTime,
        DateTime referenceTime)
    {
        if (!releaseVersion.PublishScheduled.HasValue ||
            releaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved)
        {
            return PreReleaseAccess.NoneSet;
        }

        if (referenceTime.IsBefore(startTime))
        {
            return PreReleaseAccess.Before;
        }

        return releaseVersion.Live ? PreReleaseAccess.After : PreReleaseAccess.Within;
    }
}
