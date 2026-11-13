using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public record DataSetDocumentPublicationDto
{
    public Guid? PublicationId { get; init; }
    public string? PublicationSlug { get; init; }

    public DataSetDocumentPublication ToModel() =>
        new()
        {
            PublicationId = PublicationId.ThrowIfBlank(nameof(PublicationId)),
            PublicationSlug = PublicationSlug.ThrowIfBlank(nameof(PublicationSlug)),
        };
}
