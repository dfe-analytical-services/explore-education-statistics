using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetDetailsViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public DataSetDetailsFileViewModel File { get; init; } = null!;

    public DataSetDetailsReleaseViewModel Release { get; init; } = null!;
}

public record DataSetDetailsPublicationViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string ThemeTitle { get; init; } = string.Empty;
}

public record DataSetDetailsReleaseViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; init; }

    public bool IsLatestPublishedRelease { get; init; }

    public DateTime Published { get; init; }

    public DataSetDetailsPublicationViewModel Publication { get; init; } = null!;
}

public record DataSetDetailsFileViewModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Size { get; init; } = string.Empty;
}
