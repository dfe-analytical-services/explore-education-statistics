using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

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
            return release.IsLatestPublishedVersionOfRelease() 
                   || includedReleaseIds != null && includedReleaseIds.Contains(release.Id);
        }
    }
}