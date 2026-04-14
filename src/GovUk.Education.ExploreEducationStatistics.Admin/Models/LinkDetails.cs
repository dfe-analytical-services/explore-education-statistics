#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public record LinkDetails(
    string PublicationTitle,
    string PublicationSlug,
    string ReleaseSlug,
    string? Heading,
    string Url,
    string LinkText
)
{
    public static LinkDetails FromContentBodyDetails(ContentBodyDetails data, string url, string linkText)
    {
        return new LinkDetails(
            data.PublicationTitle,
            data.PublicationSlug,
            data.Slug,
            data.SectionHeading,
            url,
            linkText
        );
    }
}

public record LinkDetailsStatusCode(int StatusCode, LinkDetails Details)
{
    public LinksCsvItem ToLinksCsvItem() =>
        new(
            Details.PublicationTitle,
            Details.Heading ?? "",
            Details.ReleaseSlug,
            Details.PublicationSlug,
            Details.Url,
            Details.LinkText,
            StatusCode
        );
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
    string PublicationTitle,
    string? SectionHeading,
    string ReleaseSlug,
    string PublicationSlug,
    string Url,
    string LinkText,
    int StatusCode = 0
)
{
    public string PublicUrl => $"/find-statistics/{PublicationSlug}/{ReleaseSlug}";
}

public record StartLinkResponse(Guid Id, string StatusUrl);
