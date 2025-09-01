using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;

public class PublicationContactDtoBuilder
{
    private Guid? _id;
    private string? _contactName;
    private string? _contactTelNo;
    private string? _teamName;
    private string? _teamEmail;

    public PublicationContactDto Build() => new()
    {
        Id = _id ?? Guid.NewGuid(),
        ContactName = _contactName ?? "Contact name",
        ContactTelNo = _contactTelNo ?? "Contact tel no",
        TeamEmail = _teamName ?? "Team email",
        TeamName = _teamEmail ?? "Team name"
    };

    public PublicationContactDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PublicationContactDtoBuilder WithContactName(string contactName)
    {
        _contactName = contactName;
        return this;
    }

    public PublicationContactDtoBuilder WithContactTelNo(string contactTelNo)
    {
        _contactTelNo = contactTelNo;
        return this;
    }

    public PublicationContactDtoBuilder WithTeamEmail(string teamEmail)
    {
        _teamEmail = teamEmail;
        return this;
    }

    public PublicationContactDtoBuilder WithTeamName(string teamName)
    {
        _teamName = teamName;
        return this;
    }
}
