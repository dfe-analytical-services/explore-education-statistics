using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Extensions
{
    public static class PublisherExtensions
    {
        /// <summary>
        /// Determines whether a Release should be published or not.
        /// </summary>
        /// <param name="release">The Release to test</param>
        /// <param name="includedReleaseIds">Release id's which are not published yet but are in the process of being published</param>
        /// <returns>True if the Release is the latest published version of a Release or is one of the included Releases</returns>
        public static bool IsReleasePublished(this Release release, IEnumerable<Guid> includedReleaseIds = null)
        {
            return includedReleaseIds != null && includedReleaseIds.Contains(release.Id) || 
                   release.IsLatestPublishedVersionOfRelease(includedReleaseIds);
        }
        
        private static bool IsLatestPublishedVersionOfRelease(this Release release, IEnumerable<Guid> includedReleaseIds)
        {
            if (release.Publication?.Releases == null || !release.Publication.Releases.Any())
            {
                throw new ArgumentException(
                    "Release must be hydrated with Publications Releases to test the latest published version");
            }

            return
                // Release itself must be live
                release.Live
                // It must also be the latest version unless the later version is a draft not included for publishing
                && !release.Publication.Releases.Any(r =>
                    (r.Live || includedReleaseIds != null && includedReleaseIds.Contains(r.Id))
                    && r.PreviousVersionId == release.Id
                    && r.Id != release.Id);
        }
    }
}