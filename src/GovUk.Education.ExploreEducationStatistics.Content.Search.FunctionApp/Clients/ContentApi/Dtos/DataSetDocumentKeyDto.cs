using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public record DataSetDocumentKeyDto
{
    public Guid? PublicationId { get; init; }
    public Guid? ReleaseId { get; init; }
    public Guid? FileId { get; init; }

    public DataSetDocumentKey ToModel() =>
        new()
        {
            PublicationId = PublicationId.ThrowIfBlank(nameof(PublicationId)),
            ReleaseId = ReleaseId.ThrowIfBlank(nameof(ReleaseId)),
            FileId = FileId.ThrowIfBlank(nameof(FileId)),
        };
}
