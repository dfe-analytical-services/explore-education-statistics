using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
                PreReleaseWindowStartTime = GetAccessWindowStart(publishScheduled),
                PreReleaseWindowEndTime = GetAccessWindowEnd(publishScheduled)
            };
        }

        public PreReleaseWindowStatus GetPreReleaseWindowStatus(Release release, DateTime referenceTime)
        {
            if (!release.PublishScheduled.HasValue)
            {
                return new PreReleaseWindowStatus
                {
                    PreReleaseAccess = PreReleaseAccess.NoneSet
                };
            }

            var publishScheduled = release.PublishScheduled.Value;
            var accessWindowStart = GetAccessWindowStart(publishScheduled);
            var accessWindowEnd = GetAccessWindowEnd(publishScheduled);

            return new PreReleaseWindowStatus
            {
                PreReleaseWindowStartTime = accessWindowStart,
                PreReleaseWindowEndTime = accessWindowEnd,
                PreReleaseAccess = GetPreReleaseAccess(release, accessWindowStart, accessWindowEnd, referenceTime)
            };
        }

        private DateTime GetAccessWindowStart(DateTime publishScheduled)
        {
            return publishScheduled.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeStart);
        }

        private DateTime GetAccessWindowEnd(DateTime publishScheduled)
        {
            return publishScheduled.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeEnd);
        }

        private static PreReleaseAccess GetPreReleaseAccess(
            Release release,
            DateTime accessWindowStart,
            DateTime accessWindowEnd,
            DateTime referenceTime)
        {
            if (!release.PublishScheduled.HasValue || release.Status != ReleaseStatus.Approved)
            {
                return PreReleaseAccess.NoneSet;
            }

            if (referenceTime.CompareTo(accessWindowStart) < 0)
            {
                return PreReleaseAccess.Before;
            }

            if (referenceTime.CompareTo(accessWindowEnd) >= 0)
            {
                return PreReleaseAccess.After;
            }

            return PreReleaseAccess.Within;
        }
    }

    public class PreReleaseOptions
    {
        public PreReleaseAccessOptions PreReleaseAccess { get; set; }
    }

    public class AccessWindowOptions
    {
        public int MinutesBeforeReleaseTimeStart { get; set; }

        public int MinutesBeforeReleaseTimeEnd { get; set; }
    }

    public class PreReleaseAccessOptions
    {
        public AccessWindowOptions AccessWindow { get; set; }
    }
}