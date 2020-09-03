using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Utils
{
    public static class PublisherUtils
    {
        public static bool IsDevelopment()
        {
            var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            return environment?.Equals(EnvironmentName.Development) ?? false;
        }
        
        public static bool IsLatestVersionOfRelease(IEnumerable<Release> releases, Guid releaseId, IEnumerable<Guid> includedReleaseIds)
        {
            return !releases.Any(r => r.PreviousVersionId == releaseId && IsReleasePublished(r, includedReleaseIds) && r.Id != releaseId);
        }
        
        public static bool IsReleasePublished(Release release, IEnumerable<Guid> includedReleaseIds)
        {
            return release.Live || includedReleaseIds.Contains(release.Id);
        }
    }
}