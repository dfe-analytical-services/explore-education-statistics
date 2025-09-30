using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;

public class PublicationMethodologiesDtoBuilder
{
    private PublicationMethodologyDto[] _methodologies = [];
    private PublicationExternalMethodologyDto? _externalMethodology;

    public PublicationMethodologiesDto Build() => new()
    {
        Methodologies = _methodologies,
        ExternalMethodology = _externalMethodology
    };

    public PublicationMethodologiesDtoBuilder WithMethodologies(PublicationMethodologyDto[] methodologies)
    {
        _methodologies = methodologies;
        return this;
    }

    public PublicationMethodologiesDtoBuilder WithExternalMethodology(
        PublicationExternalMethodologyDto? externalMethodology)
    {
        _externalMethodology = externalMethodology;
        return this;
    }
}

public class PublicationMethodologyDtoBuilder
{
    private Guid _methodologyId = Guid.NewGuid();
    private string _slug = "Slug";
    private string _title = "Title";

    public PublicationMethodologyDto Build() => new()
    {
        MethodologyId = _methodologyId,
        Slug = _slug,
        Title = _title
    };

    public PublicationMethodologyDtoBuilder WithMethodologyId(Guid methodologyId)
    {
        _methodologyId = methodologyId;
        return this;
    }

    public PublicationMethodologyDtoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public PublicationMethodologyDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}

public class PublicationExternalMethodologyDtoBuilder
{
    private string _title = "Title";
    private string _url = "Url";

    public PublicationExternalMethodologyDto Build() => new()
    {
        Title = _title,
        Url = _url
    };

    public PublicationExternalMethodologyDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public PublicationExternalMethodologyDtoBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }
}
