using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;

public class PublicationTitleDtoBuilder
{
    private Guid _publicationId = Guid.NewGuid();

    private string _title = "Title";

    public PublicationTitleDto Build() => new() { PublicationId = _publicationId, Title = _title };

    public PublicationTitleDtoBuilder WithPublicationId(Guid publicationId)
    {
        _publicationId = publicationId;
        return this;
    }

    public PublicationTitleDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}
