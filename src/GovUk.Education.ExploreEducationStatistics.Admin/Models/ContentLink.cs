#nullable enable
using CsvHelper.Configuration.Attributes;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public record ContentLink(
    string PublicationTitle,
    string PublicationSlug,
    string ReleaseSlug,
    string? Heading,
    string Url,
    string LinkText
)
{
    public static ContentLink FromContentBodyDetails(ContentBodyDetails data, string url, string linkText)
    {
        return new ContentLink(
            data.PublicationTitle,
            data.PublicationSlug,
            data.Slug,
            data.SectionHeading,
            url,
            linkText
        );
    }
}

public record TestedContentLink(CurrentEnvironment Environment, int StatusCode, bool? AnchorExists, ContentLink Details)
{
    public LinksCsvItem ToLinksCsvItem()
    {
        var publicUrlBase = Environment switch
        {
            CurrentEnvironment.Staging => "https://dev.explore-education-statistics.service.gov.uk/",
            CurrentEnvironment.Prod => "https://explore-education-statistics.service.gov.uk/",
            _ => "http://localhost:3000",
        };
        return new(
            Details.PublicationTitle,
            Details.Heading ?? "",
            Details.ReleaseSlug,
            Details.PublicationSlug,
            Details.Url,
            Details.LinkText,
            publicUrlBase,
            StatusCode,
            AnchorExists
        );
    }
}

public record ContentBodyDetails(
    string? Body,
    string PublicationTitle,
    ContentSection? Section,
    string Slug,
    string PublicationSlug
)
{
    public string? SectionHeading => Section?.Heading;
}

public record LinksCsvItem(
    [property: Index(1)] string PublicationTitle,
    [property: Index(2)] string? SectionHeading,
    [property: Index(9)] string ReleaseSlug,
    [property: Index(8)] string PublicationSlug,
    [property: Index(4)] string BrokenUrl,
    [property: Index(3)] string BrokenLinkText,
    [property: Ignore] string BaseUrl,
    [property: Index(6)] int StatusCode = 0,
    [property: Index(7)] bool? AnchorExists = null
)
{
    [property: Index(5)]
    public string PublicUrl => $"{BaseUrl}/find-statistics/{PublicationSlug}/{ReleaseSlug}";
}

public record StartLinkResponse(Guid Id, string StatusUrl);

public enum CurrentEnvironment
{
    Local,
    Staging,
    Prod,
}
