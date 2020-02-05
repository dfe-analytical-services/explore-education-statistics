using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Extensions.Options;
using static System.DateTime;

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
            if (!release.PublishScheduled.HasValue || release.Status != ReleaseStatus.Approved)
            {
                return PreReleaseWindowStatus.NoneSet;
            }
            
            var publishDate = release.PublishScheduled.GetValueOrDefault();
            var accessWindowStart = publishDate.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeStart);
            var accessWindowEnd = publishDate.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeEnd);

            if (Now.CompareTo(accessWindowStart) < 0)
            {
                return PreReleaseWindowStatus.Before;
            }

            if (Now.CompareTo(accessWindowEnd) >= 0)
            {
                return PreReleaseWindowStatus.After;
            }

            return PreReleaseWindowStatus.Within;
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