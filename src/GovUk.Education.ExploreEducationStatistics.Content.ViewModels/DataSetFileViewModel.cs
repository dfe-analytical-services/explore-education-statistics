using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetFileViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public DataSetFileFileViewModel File { get; init; } = null!;

    public DataSetFileReleaseViewModel Release { get; init; } = null!;

    public DataSetFileMetaViewModel Meta { get; init; } = null!;
}

public record DataSetFilePublicationViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string ThemeTitle { get; init; } = string.Empty;
}

public record DataSetFileReleaseViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; init; }

    public bool IsLatestPublishedRelease { get; init; }

    public DateTime Published { get; init; }

    public DataSetFilePublicationViewModel Publication { get; init; } = null!;
}

public record DataSetFileFileViewModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Size { get; init; } = string.Empty;
}
