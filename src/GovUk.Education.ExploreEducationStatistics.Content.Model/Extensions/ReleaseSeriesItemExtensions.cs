using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class ReleaseSeriesItemExtensions
{
    /// <summary>
    /// Retrieves the release id's by the order they appear in a <see cref="List{T}"/> of type <see cref="ReleaseSeriesItem"/>,
    /// ignoring any legacy links.
    /// </summary>
    /// <param name="releaseSeriesItems"></param>
    /// <returns>A <see cref="IReadOnlyList{T}"/> of type <see cref="Guid"/> containing the release id's in the order
    /// they appear in the release series.</returns>
    public static IReadOnlyList<Guid> ReleaseIds(this List<ReleaseSeriesItem> releaseSeriesItems)
    {
        return releaseSeriesItems
            .Where(rsi => !rsi.IsLegacyLink)
            .Select(rs => rs.ReleaseId!.Value)
            .ToList();
    }
}
