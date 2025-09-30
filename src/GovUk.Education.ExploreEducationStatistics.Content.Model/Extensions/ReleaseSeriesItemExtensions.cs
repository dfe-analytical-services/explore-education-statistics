#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class ReleaseSeriesItemExtensions
{
    /// <summary>
    /// Retrieves the first release id in a <see cref="List{T}"/> of type <see cref="ReleaseSeriesItem"/>,
    /// ignoring any legacy links.
    /// </summary>
    /// <param name="releaseSeriesItems"></param>
    /// <returns>The first release id in the release series.</returns>
    public static Guid? LatestReleaseId(this List<ReleaseSeriesItem> releaseSeriesItems) =>
        SelectReleaseIds(releaseSeriesItems).FirstOrDefault();

    /// <summary>
    /// Retrieves the release id's in a <see cref="List{T}"/> of type <see cref="ReleaseSeriesItem"/>,
    /// ignoring any legacy links.
    /// </summary>
    /// <param name="releaseSeriesItems"></param>
    /// <returns>A <see cref="IReadOnlyList{T}"/> of type <see cref="Guid"/> containing the release id's in the order
    /// they appear in the release series.</returns>
    public static IReadOnlyList<Guid> ReleaseIds(this List<ReleaseSeriesItem> releaseSeriesItems) =>
        SelectReleaseIds(releaseSeriesItems).ToList();

    private static IEnumerable<Guid> SelectReleaseIds(List<ReleaseSeriesItem> releaseSeriesItems) =>
        releaseSeriesItems
            .Where(rsi => !rsi.IsLegacyLink)
            .Select(rs => rs.ReleaseId!.Value);
}
