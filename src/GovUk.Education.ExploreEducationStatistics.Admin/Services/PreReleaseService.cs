using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PreReleaseService : IPreReleaseService
    {
        private readonly AccessWindowOptions _preReleaseOptions;

        public PreReleaseService(IOptions<PreReleaseOptions> config)
        {
            _preReleaseOptions = config.Value.PreReleaseAccess.AccessWindow;
        }

        public PreReleaseWindow GetPreReleaseWindow(Release release)
        {
            if (!release.PublishScheduled.HasValue)
            {
                throw new ArgumentException("Release has no PublishScheduled value", nameof(release));
            }

            var publishScheduled = release.PublishScheduled.Value;

            return new PreReleaseWindow
            {
                Start = GetStartTime(publishScheduled),
                ScheduledPublishDate = publishScheduled
            };
        }

        public PreReleaseWindowStatus GetPreReleaseWindowStatus(Release release, DateTime referenceTime)
        {
            if (release.Live)
            {
                return new PreReleaseWindowStatus
                {
                    Access = PreReleaseAccess.After
                };
            }

            if (!release.PublishScheduled.HasValue)
            {
                return new PreReleaseWindowStatus
                {
                    Access = PreReleaseAccess.NoneSet
                };
            }

            var publishScheduled = release.PublishScheduled.Value;
            var startTime = GetStartTime(publishScheduled);

            return new PreReleaseWindowStatus
            {
                Start = GetStartTime(publishScheduled),
                ScheduledPublishDate = publishScheduled,
                Access = GetAccess(release, startTime, referenceTime)
            };
        }

        private DateTime GetStartTime(DateTime publishScheduled)
        {
            return publishScheduled.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeStart);
        }

        private static PreReleaseAccess GetAccess(
            Release release,
            DateTime startTime,
            DateTime referenceTime)
        {
            if (!release.PublishScheduled.HasValue || release.ApprovalStatus != ReleaseApprovalStatus.Approved)
            {
                return PreReleaseAccess.NoneSet;
            }

            if (referenceTime.IsBefore(startTime))
            {
                return PreReleaseAccess.Before;
            }

            return release.Live ? PreReleaseAccess.After : PreReleaseAccess.Within;
        }
    }

    public class PreReleaseOptions
    {
        public PreReleaseAccessOptions PreReleaseAccess { get; set; }
    }

    public class AccessWindowOptions
    {
        public int MinutesBeforeReleaseTimeStart { get; set; }
    }

    public class PreReleaseAccessOptions
    {
        public AccessWindowOptions AccessWindow { get; set; }
    }
}
