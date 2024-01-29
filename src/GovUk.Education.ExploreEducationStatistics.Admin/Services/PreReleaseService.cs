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
                End = GetEndTime(publishScheduled)
            };
        }

        public PreReleaseWindowStatus GetPreReleaseWindowStatus(Release release, DateTime referenceTime)
        {
            if (!release.PublishScheduled.HasValue)
            {
                return new PreReleaseWindowStatus
                {
                    Access = release.Live ? PreReleaseAccess.After : PreReleaseAccess.NoneSet
                };
            }

            var publishScheduled = release.PublishScheduled.Value;
            var startTime = GetStartTime(publishScheduled);
            var endTime = GetEndTime(publishScheduled);
            var publishDayEndTime = GetPublishDayLenienceDeadline(publishScheduled);

            return new PreReleaseWindowStatus
            {
                Start = startTime,
                End = endTime,
                PublishDayLenienceDeadline = publishDayEndTime,
                Access = GetAccess(release, startTime, endTime, publishDayEndTime, referenceTime)
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

        private DateTime GetPublishDayLenienceDeadline(DateTime publishScheduled)
        {
            var ukTimeZone = DateTimeExtensions.GetUkTimeZone();

            // The Azure Functions which publish releases will do so at 9:30 in the current timezone (BST in summer, GMT otherwise)
            //      (^ for reasons explained in the CheckPublishDateCanBeScheduled method in the ReleaseApprovalService).
            // This means that during winter, these jobs will actually run at 8:30 UTC.
            // These date calculations are done in UTC, so in summer we only want to add 510 minutes onto midnight rather than 570.
            //      (^ Assuming the default publishing time of 09:30am)
            if (ukTimeZone.IsDaylightSavingTime(publishScheduled))
            {
                var adjustedMinutes = Math.Max(_preReleaseOptions.MinutesIntoPublishDayByWhichPublishingHasOccurred - 60, 0);
                return publishScheduled.AddMinutes(adjustedMinutes);
            }

            return publishScheduled.AddMinutes(_preReleaseOptions.MinutesIntoPublishDayByWhichPublishingHasOccurred);
        }

        private static PreReleaseAccess GetAccess(
            Release release,
            DateTime startTime,
            DateTime endTime,
            DateTime publishDayLenienceDeadline,
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

            if (referenceTime.IsBefore(endTime))
            {
                return PreReleaseAccess.Within;
            }

            if (referenceTime.IsAfterInclusive(endTime) && referenceTime.IsBefore(publishDayLenienceDeadline))
            {
                return PreReleaseAccess.WithinPublishDayLenience;
            }

            return PreReleaseAccess.After;
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

        public int MinutesIntoPublishDayByWhichPublishingHasOccurred { get; set; }
    }

    public class PreReleaseAccessOptions
    {
        public AccessWindowOptions AccessWindow { get; set; }
    }
}
