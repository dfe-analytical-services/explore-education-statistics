using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

/// <summary>
/// Used as the return type for the Content API's <c>ReleaseController.ListReleases</c> endpoint,
/// and as the type of the <see cref="ReleaseFileViewModel.Release"/> property.
/// </summary>
public record ReleaseSummaryViewModel
{
    /// <summary>
    /// The ReleaseVersion id
    /// </summary>
    public required Guid Id { get; init; }

    public required Guid ReleaseId { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string YearTitle { get; init; }

    public required string CoverageTitle { get; init; }

    public DateTimeOffset? Published { get; init; }

    public PartialDate? NextReleaseDate { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter<ReleaseType>))]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public required ReleaseType Type { get; init; }

    public required bool LatestRelease { get; init; }

    public PublicationSummaryViewModel? Publication { get; init; }

    // ReSharper disable once UnusedMember.Global
    // Used by JSON serialisation.
    public ReleaseSummaryViewModel() { }

    [SetsRequiredMembers]
    public ReleaseSummaryViewModel(ReleaseVersion releaseVersion, bool latestPublishedRelease)
    {
        Id = releaseVersion.Id;
        ReleaseId = releaseVersion.Release.Id;
        Title = releaseVersion.Release.Title;
        Slug = releaseVersion.Release.Slug;
        YearTitle = releaseVersion.Release.YearTitle;
        CoverageTitle = releaseVersion.Release.TimePeriodCoverage.GetEnumLabel();
        Published = releaseVersion.PublishedDisplayDate;
        NextReleaseDate = releaseVersion.NextReleaseDate;
        Type = releaseVersion.Type;
        LatestRelease = latestPublishedRelease;
    }
}
