using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;

public static class PublisherExtensions
{
    /// <summary>
    /// Determines whether a release version should be published or not.
    /// </summary>
    /// <param name="releaseVersion">The <see cref="ReleaseVersion"/> to test</param>
    /// <param name="includedReleaseVersionIds">Release version id's which are not published yet but are in the process of being published</param>
    /// <returns>True if the release version is the latest published version of a release or is one of the included releases</returns>
    public static bool IsReleasePublished(this ReleaseVersion releaseVersion,
        IEnumerable<Guid> includedReleaseVersionIds = null)
    {
        return includedReleaseVersionIds != null &&
               includedReleaseVersionIds.Contains(releaseVersion.Id) ||
               releaseVersion.IsLatestPublishedVersionOfRelease(includedReleaseVersionIds);
    }

    private static bool IsLatestPublishedVersionOfRelease(this ReleaseVersion releaseVersion,
        IEnumerable<Guid> includedReleaseIds)
    {
        if (releaseVersion.Publication?.Releases == null || !releaseVersion.Publication.Releases.Any())
        {
            throw new ArgumentException(
                "All release versions of the publication must be hydrated to test the latest published version");
        }

        return
            // Release version itself must be live
            releaseVersion.Live
            // It must also be the latest version unless the later version is a draft not included for publishing
            && !releaseVersion.Publication.Releases.Any(rv =>
                (rv.Live || includedReleaseIds != null && includedReleaseIds.Contains(rv.Id))
                && rv.PreviousVersionId == releaseVersion.Id
                && rv.Id != releaseVersion.Id);
    }
}
