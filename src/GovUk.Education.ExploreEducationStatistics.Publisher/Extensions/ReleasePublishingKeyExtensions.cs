using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;

public static class ReleasePublishingKeyExtensions
{
    public static Guid[] ToReleaseVersionIds(this IEnumerable<ReleasePublishingKey> keys) =>
        keys.Select(key => key.ReleaseVersionId).ToArray();

    public static string ToReleaseVersionIdsString(this IEnumerable<ReleasePublishingKey> keys) =>
        string.Join(",", keys.ToReleaseVersionIds());
}
