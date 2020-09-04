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

        /// <summary>
        /// Determines whether a Release will be the latest published version of a Release amongst a collection of Releases, i.e. no newer published amendments of that release exist or are about to be published.
        /// </summary>
        /// <param name="releases">Collection of Releases</param>
        /// <param name="releaseId">The Release to test</param>
        /// <param name="includedReleaseIds">Release id's which are not published yet but are in the process of being published</param>
        /// <returns>True if the there's no published Release or included Releases referencing the specified Release as a previous version</returns>
        public static bool IsLatestVersionOfRelease(IEnumerable<Release> releases, Release release, IEnumerable<Guid> includedReleaseIds)
        {
            if (!IsReleasePublished(release, includedReleaseIds))
            {
                return false;
            }

            return !releases.Any(r => r.PreviousVersionId == release.Id && IsReleasePublished(r, includedReleaseIds) && r.Id != release.Id);
        }

        /// <summary>
        /// Determines whether a Release is published or not.
        /// </summary>
        /// <param name="release">The Release to test</param>
        /// <param name="includedReleaseIds">Release id's which are not published yet but are in the process of being published</param>
        /// <returns>True if the Release is published or is one of the included Releases</returns>
        public static bool IsReleasePublished(Release release, IEnumerable<Guid> includedReleaseIds)
        {
            return release.Live || includedReleaseIds.Contains(release.Id);
        }
    }
}