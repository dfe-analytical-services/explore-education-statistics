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
                Start = GetStartTime(publishScheduled),
                End = GetEndTime(publishScheduled)
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
            var endTime = GetEndTime(publishScheduled);

            return new PreReleaseWindowStatus
            {
                Start = startTime,
                End = endTime,
                Access = GetAccess(release, startTime, endTime, referenceTime)
            };
        }

        private DateTime GetStartTime(DateTime publishScheduled)
        {
            return publishScheduled.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeStart);
        }

        private DateTime GetEndTime(DateTime publishScheduled)
        {
            return publishScheduled.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeEnd);
        }

        private static PreReleaseAccess GetAccess(
            Release release,
            DateTime startTime,
            DateTime endTime,
            DateTime referenceTime)
        {
            if (!release.PublishScheduled.HasValue || release.ApprovalStatus != ReleaseApprovalStatus.Approved)
            {
                return PreReleaseAccess.NoneSet;
            }

            if (referenceTime.CompareTo(startTime) < 0)
            {
                return PreReleaseAccess.Before;
            }

            if (referenceTime.CompareTo(endTime) >= 0)
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