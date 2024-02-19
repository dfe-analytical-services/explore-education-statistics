using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetDetailsViewModel
{
    public string Title { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public DataSetDetailsFileViewModel File { get; set; } = null!;
    public DataSetDetailsReleaseViewModel Release { get; set; } = null!;
}

public record DataSetDetailsPublicationViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string ThemeTitle { get; set; } = null!;
}

public record DataSetDetailsReleaseViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; set; }

    public bool IsLatestPublishedRelease { get; set; }

    public DateTime Published { get; set; }

    public DataSetDetailsPublicationViewModel Publication { get; set; } = null!;
}

public record DataSetDetailsFileViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Size { get; set; } = null!;
}
