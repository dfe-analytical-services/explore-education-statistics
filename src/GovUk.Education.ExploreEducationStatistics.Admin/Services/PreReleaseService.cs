using System;
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

        public PreReleaseWindowStatus GetPreReleaseWindowStatus(Release release, DateTime referenceTime)
        {
            var publishDate = release.PublishScheduled.GetValueOrDefault();
            var accessWindowStart = publishDate.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeStart);
            var accessWindowEnd = publishDate.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeEnd);

            return new PreReleaseWindowStatus
            {
                PreReleaseWindowStartTime = accessWindowStart,
                PreReleaseWindowEndTime = accessWindowEnd,
                PreReleaseAccess = GetPreReleaseAccess(release, accessWindowStart, accessWindowEnd, referenceTime)
            };
        }

        private PreReleaseAccess GetPreReleaseAccess(
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