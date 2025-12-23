using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetFileViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required DataSetFileFileViewModel File { get; init; }

    public required DataSetFileReleaseViewModel Release { get; init; }

    public required List<FootnoteViewModel> Footnotes { get; init; } = [];

    public DataSetFileApiViewModel? Api { get; set; }
}

public record DataSetFilePublicationViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string ThemeTitle { get; init; }
}

public record DataSetFileReleaseViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required ReleaseType Type { get; init; }

    public required bool IsLatestPublishedRelease { get; init; }

    public required bool IsSuperseded { get; init; }

    public required DateTimeOffset Published { get; init; }

    public DateTime LastUpdated { get; init; }

    public required DataSetFilePublicationViewModel Publication { get; init; }
}

public record DataSetFileFileViewModel
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Size { get; init; } = string.Empty;

    public required DataSetFileMetaViewModel Meta { get; init; }

    public required DataSetFileCsvPreviewViewModel DataCsvPreview { get; init; } = new();

    public required List<LabelValue> Variables { get; init; } = [];

    public required Guid SubjectId { get; init; }
}
