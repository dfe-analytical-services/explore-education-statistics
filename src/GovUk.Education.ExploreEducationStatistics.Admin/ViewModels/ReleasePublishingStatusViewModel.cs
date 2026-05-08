#nullable enable
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ReleasePublishingStatusViewModel
{
    public required Guid ReleaseId { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required ReleasePublishingStatusFilesStage FilesStage { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required ReleasePublishingStatusPublishingStage PublishingStage { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required ReleasePublishingStatusOverallStage OverallStage { get; init; }

    public required DateTimeOffset LastUpdated { get; init; }

    public static ReleasePublishingStatusViewModel FromReleasePublishingStatus(
        ReleasePublishingStatus releasePublishingStatus
    ) =>
        new()
        {
            ReleaseId = Guid.Parse(releasePublishingStatus.PartitionKey),
            FilesStage = Enum.Parse<ReleasePublishingStatusFilesStage>(releasePublishingStatus.FilesStage),
            PublishingStage = Enum.Parse<ReleasePublishingStatusPublishingStage>(
                releasePublishingStatus.PublishingStage
            ),
            OverallStage = Enum.Parse<ReleasePublishingStatusOverallStage>(releasePublishingStatus.OverallStage),
            LastUpdated = releasePublishingStatus.Timestamp ?? DateTimeOffset.MinValue,
        };
}
